using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetSubmittedHomework;

internal sealed class GetSubmittedHomeworkHandler : IQueryHandler<GetSubmittedHomeworkQuery, GetSubmittedHomeworkQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public GetSubmittedHomeworkHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<GetSubmittedHomeworkQueryResponse> ExecuteAsync(GetSubmittedHomeworkQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var submission = await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetAsync(query.SubmittedHomeworkId, cancellationToken);
        if (submission is null || submission.StudentId != query.StudentId)
            throw new NotFoundException();

        var files = await unitOfWork.ReadOnlySubmittedHomeworkFileRepository.ListBySubmittedHomeworkAsync(query.SubmittedHomeworkId, cancellationToken);
        var reviews = await unitOfWork.ReadOnlySubmittedReviewRepository.ListBySubmittedHomeworkAsync(query.SubmittedHomeworkId, cancellationToken);
        var submittedHomeworkMark = await unitOfWork.ReadOnlySubmittedHomeworkMarkRepository.GetBySubmittedHomeworkAsync(query.SubmittedHomeworkId, cancellationToken);

        return new GetSubmittedHomeworkQueryResponse
        {
            SubmittedHomework = submission,
            Files = files,
            SubmittedReviews = reviews,
            FinalMark = GetFinalMark(submittedHomeworkMark)
        };
    }

    private static int? GetFinalMark(SubmittedHomeworkMark? submittedHomeworkMark)
    {
        if (submittedHomeworkMark is null)
        {
            return null;
        }

        if (submittedHomeworkMark.TeacherMark is { } teacherMark)
        {
            return teacherMark;
        }

        return submittedHomeworkMark.ReviewersMark;
    }
}
