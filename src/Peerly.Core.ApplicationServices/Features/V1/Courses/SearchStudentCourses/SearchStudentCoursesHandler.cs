using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Teachers;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.SearchStudentCourses;

internal sealed class SearchStudentCoursesHandler : IQueryHandler<SearchStudentCoursesQuery, SearchStudentCoursesQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public SearchStudentCoursesHandler(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<SearchStudentCoursesQueryResponse> ExecuteAsync(SearchStudentCoursesQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var courseIds = await unitOfWork.ReadOnlyGroupRepository.ListCourseIdsAsync(query.StudentId, cancellationToken);
        if (courseIds.Count == 0)
            return new SearchStudentCoursesQueryResponse { Courses = [], TeachersByCourseId = new Dictionary<CourseId, IReadOnlyCollection<Teacher>>() };

        var courseFilter = new CourseFilter
        {
            CourseIds = courseIds,
            CourseStatuses = query.Filter.CourseStatuses
        };
        var courses = await unitOfWork.ReadOnlyCourseRepository.ListAsync(courseFilter, query.PaginationInfo, cancellationToken);
        var notDeletedCourses = courses.Where(course => course.Status != CourseStatus.Deleted).ToArray();
        if (notDeletedCourses.Length == 0)
            return new SearchStudentCoursesQueryResponse { Courses = [], TeachersByCourseId = new Dictionary<CourseId, IReadOnlyCollection<Teacher>>() };

        var teachersByCourseId = await GetTeachersByCourseIdAsync(unitOfWork, notDeletedCourses, cancellationToken);

        return new SearchStudentCoursesQueryResponse
        {
            Courses = notDeletedCourses,
            TeachersByCourseId = teachersByCourseId
        };
    }

    private static async Task<IReadOnlyDictionary<CourseId, IReadOnlyCollection<Teacher>>> GetTeachersByCourseIdAsync(
        ICommonReadOnlyUnitOfWork unitOfWork,
        IReadOnlyCollection<Course> courses,
        CancellationToken cancellationToken)
    {
        var courseIds = courses.ToArrayBy(course => course.Id);
        var courseTeachers = await unitOfWork.ReadOnlyCourseTeacherRepository.ListAsync(courseIds, cancellationToken);

        var teacherFilter = new TeacherFilter { TeacherIds = courseTeachers.Select(courseTeacher => courseTeacher.TeacherId).ToHashSet() };
        var teachers = await unitOfWork.ReadOnlyTeacherRepository.ListAsync(teacherFilter, cancellationToken);

        var teachersById = teachers.ToDictionary(teacher => teacher.Id);
        return courseTeachers
            .Where(courseTeacher => teachersById.ContainsKey(courseTeacher.TeacherId))
            .GroupBy(courseTeacher => courseTeacher.CourseId)
            .ToDictionary(group => group.Key, ToTeachers);

        IReadOnlyCollection<Teacher> ToTeachers(IGrouping<CourseId, CourseTeacher> group)
        {
            return group.ToArrayBy(courseTeacher => teachersById[courseTeacher.TeacherId]);
        }
    }
}
