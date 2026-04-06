using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Students;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetTeacherSubmissionDetail;

internal sealed class GetTeacherSubmissionDetailHandler : IQueryHandler<GetTeacherSubmissionDetailQuery, GetTeacherSubmissionDetailQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public GetTeacherSubmissionDetailHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<GetTeacherSubmissionDetailQueryResponse> ExecuteAsync(
        GetTeacherSubmissionDetailQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

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

        var courseTeacherExistsItem = new CourseTeacherExistsItem
        {
            CourseId = homework.CourseId,
            TeacherId = query.TeacherId
        };
        var isCourseTeacher = await unitOfWork.ReadOnlyCourseTeacherRepository
            .ExistsAsync(courseTeacherExistsItem, cancellationToken);
        if (!isCourseTeacher)
        {
            throw new NotFoundException();
        }

        var fileItems = await unitOfWork.ReadOnlySubmittedHomeworkFileRepository
            .ListAsync(query.SubmittedHomeworkId, cancellationToken);
        var fileIds = fileItems.ToArrayBy(f => f.FileId);
        var files = fileIds.Length > 0
            ? await unitOfWork.ReadOnlyFileRepository.ListByIdsAsync(fileIds, cancellationToken)
            : [];

        var reviews = await unitOfWork.ReadOnlySubmittedReviewRepository
            .ListBySubmittedHomeworkIdAsync(query.SubmittedHomeworkId, cancellationToken);

        var reviewerStudentIds = reviews.ToArrayBy(r => r.StudentId);
        var submissionStudentIds = reviewerStudentIds.Append(submission.StudentId).Distinct().ToArray();

        var studentFilter = new StudentFilter { StudentIds = submissionStudentIds };
        var students = await unitOfWork.ReadOnlyStudentRepository.ListAsync(studentFilter, cancellationToken);
        var studentsByIds = students.ToDictionary(s => s.Id);

        var mark = await unitOfWork.ReadOnlySubmittedHomeworkMarkRepository
            .GetAsync(query.SubmittedHomeworkId, cancellationToken);

        studentsByIds.TryGetValue(submission.StudentId, out var submissionStudent);

        return new GetTeacherSubmissionDetailQueryResponse
        {
            SubmittedHomeworkId = (long)submission.Id,
            StudentId = (long)submission.StudentId,
            StudentName = submissionStudent?.Name ?? submissionStudent?.Email ?? string.Empty,
            Comment = submission.Comment,
            Files = files,
            Reviews = reviews.ToArrayBy(r =>
            {
                studentsByIds.TryGetValue(r.StudentId, out var reviewer);
                return new ReviewWithStudent
                {
                    Review = r,
                    Reviewer = reviewer
                };
            }),
            Mark = mark
        };
    }
}
