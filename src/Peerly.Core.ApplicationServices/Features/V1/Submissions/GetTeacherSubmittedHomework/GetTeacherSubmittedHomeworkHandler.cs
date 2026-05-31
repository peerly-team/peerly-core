using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Students;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetTeacherSubmittedHomework;

internal sealed class GetTeacherSubmittedHomeworkHandler : IQueryHandler<GetTeacherSubmittedHomeworkQuery, GetTeacherSubmittedHomeworkQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IQueryValidator<GetTeacherSubmittedHomeworkQuery, GetTeacherSubmittedHomeworkQueryResponse> _validator;

    public GetTeacherSubmittedHomeworkHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IQueryValidator<GetTeacherSubmittedHomeworkQuery, GetTeacherSubmittedHomeworkQueryResponse> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<GetTeacherSubmittedHomeworkQueryResponse> ExecuteAsync(
        GetTeacherSubmittedHomeworkQuery query,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(query, cancellationToken);

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var submittedHomeworkId = query.SubmittedHomeworkId;
        var submittedHomework = await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetAsync(submittedHomeworkId, cancellationToken)
                                ?? throw new NotFoundException();

        var files = await unitOfWork.ReadOnlySubmittedHomeworkFileRepository.ListBySubmittedHomeworkAsync(submittedHomeworkId, cancellationToken);
        var reviews = await unitOfWork.ReadOnlySubmittedReviewRepository.ListBySubmittedHomeworkAsync(submittedHomeworkId, cancellationToken);

        var reviewIds = reviews.ToArrayBy(r => r.Id);
        var allScores = await unitOfWork.ReadOnlySubmittedReviewScoreRepository.ListBySubmittedReviewIdsAsync(reviewIds, cancellationToken);
        var scoresByReview = allScores.ToLookup(s => s.SubmittedReviewId);
        var enrichedReviews = reviews.ToArrayBy(r => r with { Scores = scoresByReview[r.Id].ToArray() });

        var studentById = await GetStudentByIdAsync(unitOfWork, reviews, submittedHomework.StudentId, cancellationToken);

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, cancellationToken);
        var submittedHomeworkMark = homework!.Status is HomeworkStatus.Reviewing
            ? null
            : await unitOfWork.ReadOnlySubmittedHomeworkMarkRepository.GetBySubmittedHomeworkAsync(submittedHomeworkId, cancellationToken);

        return new GetTeacherSubmittedHomeworkQueryResponse
        {
            SubmittedHomework = submittedHomework,
            Student = studentById[submittedHomework.StudentId],
            Files = files,
            SubmittedReviews = enrichedReviews.ToArrayBy(review => review.ToTeacherSubmittedReview(studentById[review.StudentId])),
            ReviewersMark = submittedHomeworkMark?.ReviewersMark,
            TeacherMark = submittedHomeworkMark?.TeacherMark
        };
    }

    private static async Task<Dictionary<StudentId, Student>> GetStudentByIdAsync(
        ICommonReadOnlyUnitOfWork unitOfWork,
        IReadOnlyCollection<SubmittedReview> reviews,
        StudentId studentId,
        CancellationToken cancellationToken)
    {
        var studentIds = reviews
            .Select(review => review.StudentId)
            .Append(studentId)
            .ToArray();

        var studentFilter = new StudentFilter { StudentIds = studentIds };
        var students = await unitOfWork.ReadOnlyStudentRepository.ListAsync(studentFilter, cancellationToken);

        return students.ToDictionary(student => student.Id);
    }
}
