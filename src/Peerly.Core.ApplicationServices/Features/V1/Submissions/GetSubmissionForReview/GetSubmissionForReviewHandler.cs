using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using SubmittedHomeworkStudent = Peerly.Core.Models.Submissions.SubmittedHomeworkStudent;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetSubmissionForReview;

internal sealed class GetSubmissionForReviewHandler : IQueryHandler<GetSubmissionForReviewQuery, GetSubmissionForReviewQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public GetSubmissionForReviewHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<GetSubmissionForReviewQueryResponse> ExecuteAsync(
        GetSubmissionForReviewQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var isAssigned = await unitOfWork.ReadOnlyDistributionReviewerRepository.ExistsAsync(
            new SubmittedHomeworkStudent
            {
                SubmittedHomeworkId = query.SubmittedHomeworkId,
                StudentId = query.StudentId
            },
            cancellationToken);
        if (!isAssigned)
        {
            throw new NotFoundException();
        }

        var submission = await unitOfWork.ReadOnlySubmittedHomeworkRepository
            .GetAsync(query.SubmittedHomeworkId, cancellationToken);
        if (submission is null)
        {
            throw new NotFoundException();
        }

        var homework = await unitOfWork.ReadOnlyHomeworkRepository
            .GetAsync(submission.HomeworkId, cancellationToken);
        if (homework is null)
        {
            throw new NotFoundException();
        }

        var fileItems = await unitOfWork.ReadOnlySubmittedHomeworkFileRepository
            .ListAsync(query.SubmittedHomeworkId, cancellationToken);

        var anonymizedFileIds = fileItems
            .Select(f => f.AnonymizedFileId ?? f.FileId)
            .ToArray();

        var files = anonymizedFileIds.Length > 0
            ? await unitOfWork.ReadOnlyFileRepository.ListByIdsAsync(anonymizedFileIds, cancellationToken)
            : [];

        return new GetSubmissionForReviewQueryResponse
        {
            SubmittedHomeworkId = (long)query.SubmittedHomeworkId,
            Comment = submission.Comment,
            AnonymizedFiles = files,
            Checklist = homework.CheckList
        };
    }
}
