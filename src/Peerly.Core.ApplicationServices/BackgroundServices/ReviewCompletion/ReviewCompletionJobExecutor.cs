using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions.Executors;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.BackgroundService;
using Peerly.Core.Models.BackgroundService.ReviewCompletions;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.BackgroundServices.ReviewCompletion;

internal sealed class ReviewCompletionJobExecutor : IExecutor<ReviewCompletionJobItem>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IClock _clock;
    private readonly ILogger<ReviewCompletionJobExecutor> _logger;

    public ReviewCompletionJobExecutor(
        ICommonUnitOfWorkFactory unitOfWorkFactory,
        IClock clock,
        ILogger<ReviewCompletionJobExecutor> logger)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _clock = clock;
        _logger = logger;
    }

    public async Task RunAsync(ReviewCompletionJobItem requestItem, CancellationToken cancellationToken)
    {
        var homeworkId = requestItem.HomeworkId;
        await using var unitOfWork = await _unitOfWorkFactory.CreateAsync(cancellationToken);

        try
        {
            var homework = await GetHomeworkAsync(unitOfWork, requestItem, cancellationToken);
            if (homework is null)
                return;

            await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

            await AggregateReviewersMarksAsync(unitOfWork, homeworkId, cancellationToken);
            await CompleteProcessingAsync(unitOfWork, homework, HomeworkStatus.Confirmation, cancellationToken);

            await operationSet.Complete(cancellationToken);
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;

            _logger.LogError(
                ex,
                "{Job} | HomeworkId: {HomeworkId} | Job processing failed with error message: {ErrorMessage}",
                nameof(ReviewCompletionJobExecutor),
                homeworkId,
                errorMessage);

            await UpdateToFailed(unitOfWork, homeworkId, errorMessage, cancellationToken);
            return;
        }

        _logger.LogInformation(
            "{Job} | HomeworkId: {HomeworkId} | Completed the process successfully",
            nameof(ReviewCompletionJobExecutor),
            homeworkId);
    }

    private async Task<Homework?> GetHomeworkAsync(
        ICommonUnitOfWork unitOfWork,
        ReviewCompletionJobItem requestItem,
        CancellationToken cancellationToken)
    {
        var homeworkId = requestItem.HomeworkId;
        var homework = await unitOfWork.HomeworkRepository.GetAsync(homeworkId, cancellationToken);

        if (homework is null)
        {
            const string ErrorMessage = "Homework not found";

            _logger.LogWarning(
                "{Job} | HomeworkId: {HomeworkId} | {ErrorMessage}",
                nameof(ReviewCompletionJobExecutor),
                homeworkId,
                ErrorMessage);

            await UpdateProcessStatusToCancelled(unitOfWork, homeworkId, ErrorMessage, cancellationToken);
            return null;
        }

        if (homework.Status is not HomeworkStatus.Reviewing)
        {
            var errorMessage = $"Homework has invalid status: {homework.Status}";

            _logger.LogWarning(
                "{Job} | HomeworkId: {HomeworkId} | {ErrorMessage}",
                nameof(ReviewCompletionJobExecutor),
                homeworkId,
                errorMessage);

            await UpdateProcessStatusToCancelled(unitOfWork, homeworkId, errorMessage, cancellationToken);
            return null;
        }

        if (_clock.GetCurrentTime() < homework.ReviewDeadline)
        {
            _logger.LogInformation(
                "{Job} | HomeworkId: {HomeworkId} | Review deadline has not passed yet, rescheduling completion to {ReviewDeadline}",
                nameof(ReviewCompletionJobExecutor),
                homeworkId,
                homework.ReviewDeadline);

            await UpdateCompletionTime(unitOfWork, homework, cancellationToken);
            return null;
        }

        return homework;
    }

    private async Task CompleteProcessingAsync(
        ICommonUnitOfWork unitOfWork,
        Homework homework,
        HomeworkStatus homeworkStatus,
        CancellationToken cancellationToken)
    {
        await unitOfWork.ReviewCompletionRepository.UpdateAsync(
            homework.Id,
            builder =>
                builder
                    .Set(item => item.ProcessStatus, ProcessStatus.Done)
                    .Set(item => item.ProcessTime, _clock.GetCurrentTime())
                    .Set(item => item.Error, null),
            cancellationToken);

        await unitOfWork.HomeworkRepository.UpdateAsync(
            homework.Id,
            builder => builder
                .Set(item => item.Status, homeworkStatus),
            cancellationToken);
    }

    private static Task<bool> UpdateCompletionTime(
        ICommonUnitOfWork unitOfWork,
        Homework homework,
        CancellationToken cancellationToken)
    {
        return unitOfWork.ReviewCompletionRepository.UpdateAsync(
            homework.Id,
            builder =>
                builder
                    .Set(item => item.ProcessStatus, ProcessStatus.Created)
                    .Set(item => item.CompletionTime, homework.ReviewDeadline)
                    .Set(item => item.TakenTime, null),
            cancellationToken);
    }

    private Task<bool> UpdateToFailed(
        ICommonUnitOfWork unitOfWork,
        HomeworkId homeworkId,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        return unitOfWork.ReviewCompletionRepository.UpdateAsync(
            homeworkId,
            builder =>
                builder
                    .Set(item => item.ProcessStatus, ProcessStatus.Failed)
                    .Set(item => item.Error, errorMessage)
                    .Set(item => item.IncrementFailCount, true)
                    .Set(item => item.ProcessTime, _clock.GetCurrentTime()),
            cancellationToken);
    }

    private async Task AggregateReviewersMarksAsync(
        ICommonUnitOfWork unitOfWork,
        HomeworkId homeworkId,
        CancellationToken cancellationToken)
    {
        var reviewersMarks = await unitOfWork.SubmittedReviewRepository.ListSubmittedReviewMarksAsync(
            homeworkId,
            cancellationToken);

        if (reviewersMarks.Count == 0)
            return;

        var currentTime = _clock.GetCurrentTime();
        var markAddItems = reviewersMarks
            .GroupBy(rm => rm.SubmittedHomeworkId)
            .Select(group => new SubmittedHomeworkMarkAddItem
            {
                SubmittedHomeworkId = group.Key,
                ReviewersMark = (int)Math.Round(group.Average(rm => rm.ReviewerMark)),
                CreationTime = currentTime
            })
            .ToArray();

        await unitOfWork.SubmittedHomeworkMarkRepository.BatchAddAsync(markAddItems, cancellationToken);
    }

    private Task<bool> UpdateProcessStatusToCancelled(
        ICommonUnitOfWork unitOfWork,
        HomeworkId homeworkId,
        string reason,
        CancellationToken cancellationToken)
    {
        return unitOfWork.ReviewCompletionRepository.UpdateAsync(
            homeworkId,
            builder =>
                builder
                    .Set(item => item.ProcessStatus, ProcessStatus.Cancelled)
                    .Set(item => item.Error, reason)
                    .Set(item => item.ProcessTime, _clock.GetCurrentTime()),
            cancellationToken);
    }
}
