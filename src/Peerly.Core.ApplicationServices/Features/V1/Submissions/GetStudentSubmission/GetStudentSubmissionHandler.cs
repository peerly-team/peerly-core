using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetStudentSubmission;

internal sealed class GetStudentSubmissionHandler : IQueryHandler<GetStudentSubmissionQuery, GetStudentSubmissionQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public GetStudentSubmissionHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<GetStudentSubmissionQueryResponse> ExecuteAsync(
        GetStudentSubmissionQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var submission = await unitOfWork.ReadOnlySubmittedHomeworkRepository
            .GetByHomeworkAndStudentAsync(query.HomeworkId, query.StudentId, cancellationToken);
        if (submission is null)
        {
            throw new NotFoundException();
        }

        var fileItems = await unitOfWork.ReadOnlySubmittedHomeworkFileRepository
            .ListAsync(submission.Id, cancellationToken);
        var fileIds = fileItems.ToArrayBy(f => f.FileId);
        var files = fileIds.Length > 0
            ? await unitOfWork.ReadOnlyFileRepository.ListByIdsAsync(fileIds, cancellationToken)
            : [];

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(query.HomeworkId, cancellationToken);

        IReadOnlyCollection<SubmittedReview> reviews = [];
        SubmittedHomeworkMark? mark = null;

        if (homework is not null && homework.Status == HomeworkStatus.Finished)
        {
            reviews = await unitOfWork.ReadOnlySubmittedReviewRepository
                .ListBySubmittedHomeworkIdAsync(submission.Id, cancellationToken);
            mark = await unitOfWork.ReadOnlySubmittedHomeworkMarkRepository
                .GetAsync(submission.Id, cancellationToken);
        }

        return new GetStudentSubmissionQueryResponse
        {
            SubmittedHomeworkId = (long)submission.Id,
            Comment = submission.Comment,
            Files = files,
            Reviews = reviews,
            Mark = mark
        };
    }
}
