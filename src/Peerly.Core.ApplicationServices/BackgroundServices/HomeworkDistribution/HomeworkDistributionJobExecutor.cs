using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions.Executors;
using Peerly.Core.ApplicationServices.BackgroundServices.HomeworkDistribution.Abstractions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.BackgroundService;
using Peerly.Core.Models.BackgroundService.HomeworkDistributions;
using Peerly.Core.Models.BackgroundService.ReviewCompletions;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.BackgroundServices.HomeworkDistribution;

internal sealed class HomeworkDistributionJobExecutor : IExecutor<HomeworkDistributionJobItem>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IClock _clock;
    private readonly IHomeworkDistributionJobValidator _validator;
    private readonly ILogger<HomeworkDistributionJobExecutor> _logger;

    public HomeworkDistributionJobExecutor(
        ICommonUnitOfWorkFactory unitOfWorkFactory,
        IClock clock,
        IHomeworkDistributionJobValidator validator,
        ILogger<HomeworkDistributionJobExecutor> logger)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _clock = clock;
        _validator = validator;
        _logger = logger;
    }

    public async Task RunAsync(HomeworkDistributionJobItem requestItem, CancellationToken cancellationToken)
    {
        var homeworkId = requestItem.HomeworkId;
        await using var unitOfWork = await _unitOfWorkFactory.CreateAsync(cancellationToken);

        try
        {
            var homework = await GetHomeworkAsync(unitOfWork, requestItem, cancellationToken);
            if (homework is null)
                return;

            var submittedHomeworks = await GetSubmittedHomeworkStudentsAsync(unitOfWork, homework, cancellationToken);
            await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

            var distributionReviewerAddItems = GetDistributionReviewerAddItems(submittedHomeworks, homework.AmountOfReviewers);

            if (distributionReviewerAddItems.Count == 0)
            {
                await UpdateToDoneWithStatus(unitOfWork, homework, HomeworkStatus.Confirmation, cancellationToken);
            }
            else
            {
                await unitOfWork.DistributionReviewerRepository.BatchAddAsync(distributionReviewerAddItems, cancellationToken);
                await UpdateToDoneWithStatus(unitOfWork, homework, HomeworkStatus.Reviewing, cancellationToken);
            }

            await operationSet.Complete(cancellationToken);
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;

            _logger.LogError(
                ex,
                "{Job} | HomeworkId: {HomeworkId} | Job processing failed with error message: {ErrorMessage}",
                nameof(HomeworkDistributionJobExecutor),
                homeworkId,
                errorMessage);

            await UpdateToFailed(unitOfWork, homeworkId, errorMessage, cancellationToken);
            return;
        }

        _logger.LogInformation(
            "{Job} | HomeworkId: {HomeworkId} | Completed the process successfully",
            nameof(HomeworkDistributionJobExecutor),
            homeworkId);
    }

    private async Task<Homework?> GetHomeworkAsync(
        ICommonUnitOfWork unitOfWork,
        HomeworkDistributionJobItem requestItem,
        CancellationToken cancellationToken)
    {
        var homeworkId = requestItem.HomeworkId;
        var homework = await unitOfWork.HomeworkRepository.GetAsync(homeworkId, cancellationToken);

        if (homework is null)
        {
            const string ErrorMessage = "Homework not found";

            _logger.LogWarning(
                "{Job} | HomeworkId: {HomeworkId} | {ErrorMessage}",
                nameof(HomeworkDistributionJobExecutor),
                homeworkId,
                ErrorMessage);

            await UpdateProcessStatusToCancelled(unitOfWork, homeworkId, ErrorMessage, cancellationToken);
            return null;
        }

        if (homework.Status is not HomeworkStatus.Published)
        {
            var errorMessage = $"Homework has invalid status: {homework.Status}";

            _logger.LogWarning(
                "{Job} | HomeworkId: {HomeworkId} | {ErrorMessage}",
                nameof(HomeworkDistributionJobExecutor),
                homeworkId,
                errorMessage);

            await UpdateProcessStatusToCancelled(unitOfWork, homeworkId, errorMessage, cancellationToken);
            return null;
        }

        if (homework.Deadline > requestItem.DistributionTime)
        {
            _logger.LogInformation(
                "{Job} | HomeworkId: {HomeworkId} | Deadline postponed, rescheduling distribution to {Deadline}",
                nameof(HomeworkDistributionJobExecutor),
                homeworkId,
                homework.Deadline);

            await UpdateDistributionTime(unitOfWork, homework, cancellationToken);
            return null;
        }

        return homework;
    }

    private static Task<bool> UpdateDistributionTime(ICommonUnitOfWork unitOfWork, Homework homework, CancellationToken cancellationToken)
    {
        return unitOfWork.HomeworkDistributionRepository.UpdateAsync(
            homework.Id,
            builder =>
                builder
                    .Set(item => item.ProcessStatus, ProcessStatus.Created)
                    .Set(item => item.DistributionTime, homework.Deadline)
                    .Set(item => item.TakenTime, null),
            cancellationToken);
    }

    private async Task<IReadOnlyCollection<SubmittedHomeworkStudent>> GetSubmittedHomeworkStudentsAsync(
        ICommonUnitOfWork unitOfWork,
        Homework homework,
        CancellationToken cancellationToken)
    {
        var submittedHomeworkFilter = homework.ToSubmittedHomeworkFilter();
        var submittedHomeworks = await unitOfWork.SubmittedHomeworkRepository.ListSubmittedHomeworkStudentAsync(
            submittedHomeworkFilter,
            cancellationToken);

        _validator.Validate(submittedHomeworks, homework.AmountOfReviewers);

        return submittedHomeworks;
    }

    private async Task UpdateToDoneWithStatus(
        ICommonUnitOfWork unitOfWork,
        Homework homework,
        HomeworkStatus homeworkStatus,
        CancellationToken cancellationToken)
    {
        await unitOfWork.HomeworkDistributionRepository.UpdateAsync(
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

        if (homeworkStatus == HomeworkStatus.Reviewing)
        {
            await unitOfWork.ReviewCompletionRepository.AddAsync(
                new ReviewCompletionAddItem
                {
                    HomeworkId = homework.Id,
                    CompletionTime = homework.ReviewDeadline,
                    CreationTime = _clock.GetCurrentTime(),
                    ProcessStatus = ProcessStatus.Created,
                    FailCount = 0
                },
                cancellationToken);
        }
    }

    private Task<bool> UpdateToFailed(
        ICommonUnitOfWork unitOfWork,
        HomeworkId homeworkId,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        return unitOfWork.HomeworkDistributionRepository.UpdateAsync(
            homeworkId,
            builder =>
                builder
                    .Set(item => item.ProcessStatus, ProcessStatus.Failed)
                    .Set(item => item.Error, errorMessage)
                    .Set(item => item.IncrementFailCount, true)
                    .Set(item => item.ProcessTime, _clock.GetCurrentTime()),
            cancellationToken);
    }

    private Task<bool> UpdateProcessStatusToCancelled(
        ICommonUnitOfWork unitOfWork,
        HomeworkId homeworkId,
        string reason,
        CancellationToken cancellationToken)
    {
        return unitOfWork.HomeworkDistributionRepository.UpdateAsync(
            homeworkId,
            builder =>
                builder
                    .Set(item => item.ProcessStatus, ProcessStatus.Cancelled)
                    .Set(item => item.Error, reason)
                    .Set(item => item.ProcessTime, _clock.GetCurrentTime()),
            cancellationToken);
    }

    private static List<DistributionReviewerAddItem> GetDistributionReviewerAddItems(
        IReadOnlyCollection<SubmittedHomeworkStudent> submittedHomeworkStudents,
        int amountOfReviewers)
    {
        if (submittedHomeworkStudents.Count < 2)
            return [];

        var students = submittedHomeworkStudents.Shuffle().ToArray();
        var effectiveReviewers = Math.Min(amountOfReviewers, students.Length - 1);

        var result = new List<DistributionReviewerAddItem>(students.Length * effectiveReviewers);

        // Распределяем рецензентов с помощью цикличных сдвигов по перемешанному списку студентов.
        // Внешний цикл задаёт величину сдвига (1..effectiveReviewers) — каждый сдвиг
        // представляет собой один «раунд» рецензирования.
        // Внутренний цикл проходит по всем студентам: студент с индексом i проверяет работу
        // студента с индексом (i + shift) % students.Length.
        for (var shift = 1; shift <= effectiveReviewers; shift++)
        {
            for (var i = 0; i < students.Length; i++)
            {
                result.Add(
                    new DistributionReviewerAddItem
                    {
                        SubmittedHomeworkId = students[(i + shift) % students.Length].SubmittedHomeworkId,
                        StudentId = students[i].StudentId
                    });
            }
        }

        return result;
    }
}
