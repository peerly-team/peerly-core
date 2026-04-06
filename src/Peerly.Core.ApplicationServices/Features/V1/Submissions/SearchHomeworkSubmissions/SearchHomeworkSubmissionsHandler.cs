using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Students;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.SearchHomeworkSubmissions;

internal sealed class SearchHomeworkSubmissionsHandler : IQueryHandler<SearchHomeworkSubmissionsQuery, SearchHomeworkSubmissionsQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public SearchHomeworkSubmissionsHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<SearchHomeworkSubmissionsQueryResponse> ExecuteAsync(
        SearchHomeworkSubmissionsQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(query.HomeworkId, cancellationToken);
        if (homework is null)
        {
            throw new NotFoundException();
        }

        var courseTeacherExistsItem = new CourseTeacherExistsItem
        {
            CourseId = homework.CourseId,
            TeacherId = query.TeacherId
        };
        var isCourseTeacher = await unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacherExistsItem, cancellationToken);
        if (!isCourseTeacher)
        {
            throw new NotFoundException();
        }

        var submittedHomeworkFilter = new SubmittedHomeworkFilter { HomeworkIds = [query.HomeworkId] };
        var submittedHomeworkStudents = await unitOfWork.ReadOnlySubmittedHomeworkRepository
            .ListSubmittedHomeworkStudentAsync(submittedHomeworkFilter, cancellationToken);

        if (submittedHomeworkStudents.Count == 0)
        {
            return new SearchHomeworkSubmissionsQueryResponse { Submissions = [] };
        }

        var studentFilter = new StudentFilter
        {
            StudentIds = submittedHomeworkStudents.ToArrayBy(s => s.StudentId)
        };
        var students = await unitOfWork.ReadOnlyStudentRepository.ListAsync(studentFilter, cancellationToken);
        var studentsByIds = students.ToDictionary(s => s.Id);

        var marks = await unitOfWork.ReadOnlySubmittedHomeworkMarkRepository.ListAsync(query.HomeworkId, cancellationToken);
        var marksBySubmittedHomeworkId = marks.ToDictionary(m => m.SubmittedHomeworkId);

        var reviewerMarks = await unitOfWork.ReadOnlySubmittedReviewRepository.ListSubmittedReviewMarksAsync(query.HomeworkId, cancellationToken);
        var reviewsCountBySubmittedHomeworkId = reviewerMarks
            .GroupBy(rm => rm.SubmittedHomeworkId)
            .ToDictionary(g => g.Key, g => g.Count());

        var submissions = new List<SubmissionOverviewItem>(submittedHomeworkStudents.Count);
        foreach (var submittedHomeworkStudent in submittedHomeworkStudents)
        {
            var studentName = studentsByIds.TryGetValue(submittedHomeworkStudent.StudentId, out var student)
                ? student.Name ?? student.Email
                : string.Empty;

            marksBySubmittedHomeworkId.TryGetValue(submittedHomeworkStudent.SubmittedHomeworkId, out var mark);
            reviewsCountBySubmittedHomeworkId.TryGetValue(submittedHomeworkStudent.SubmittedHomeworkId, out var reviewsCount);

            submissions.Add(new SubmissionOverviewItem
            {
                SubmittedHomeworkId = (long)submittedHomeworkStudent.SubmittedHomeworkId,
                StudentId = (long)submittedHomeworkStudent.StudentId,
                StudentName = studentName,
                ReviewersMark = mark?.ReviewersMark,
                TeacherMark = mark?.TeacherMark,
                HasDiscrepancy = mark?.HasDiscrepancy ?? false,
                ReviewsReceived = reviewsCount
            });
        }

        return new SearchHomeworkSubmissionsQueryResponse
        {
            Submissions = submissions
        };
    }
}
