using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Tools;

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

        var submittedHomework = await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetAsync(query.SubmittedHomeworkId, cancellationToken);
        if (submittedHomework is null || submittedHomework.StudentId != query.StudentId)
            throw new NotFoundException();

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, cancellationToken)
                       ?? throw new NotFoundException();
        var files = await unitOfWork.ReadOnlySubmittedHomeworkFileRepository.ListBySubmittedHomeworkAsync(
            query.SubmittedHomeworkId,
            cancellationToken);
        if (homework.Status is not HomeworkStatus.Finished)
        {
            return new GetSubmittedHomeworkQueryResponse
            {
                SubmittedHomework = submittedHomework,
                Files = files,
                SubmittedReviews = [],
                FinalMark = null
            };
        }

        var submittedReviews = await GetSubmittedReviewsAsync(unitOfWork, query.SubmittedHomeworkId, cancellationToken);
        var submittedHomeworkMark =
            await unitOfWork.ReadOnlySubmittedHomeworkMarkRepository.GetBySubmittedHomeworkAsync(
                query.SubmittedHomeworkId,
                cancellationToken);

        return new GetSubmittedHomeworkQueryResponse
        {
            SubmittedHomework = submittedHomework,
            Files = files,
            SubmittedReviews = submittedReviews,
            FinalMark = GetFinalMark(submittedHomeworkMark)
        };
    }

    private static async Task<IReadOnlyCollection<SubmittedReview>> GetSubmittedReviewsAsync(
        ICommonReadOnlyUnitOfWork unitOfWork,
        SubmittedHomeworkId submittedHomeworkId,
        CancellationToken cancellationToken)
    {
        var reviews = await unitOfWork.ReadOnlySubmittedReviewRepository.ListBySubmittedHomeworkAsync(
            submittedHomeworkId,
            cancellationToken);

        var allScores = await unitOfWork.ReadOnlySubmittedReviewScoreRepository.ListBySubmittedReviewIdsAsync(
            reviews.ToArrayBy(r => r.Id),
            cancellationToken);
        var scoresBySubmittedReviewId = allScores.ToLookup(s => s.SubmittedReviewId);

        return reviews.ToArrayBy(
            r => r with
            {
                Scores = scoresBySubmittedReviewId[r.Id].ToArray()
            });
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
