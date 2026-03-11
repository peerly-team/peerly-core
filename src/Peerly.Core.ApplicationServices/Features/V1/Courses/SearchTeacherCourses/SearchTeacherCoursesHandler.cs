using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Extensions;
using Peerly.Core.ApplicationServices.Features.V1.Courses.Shared.SearchCourses;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.SearchTeacherCourses;

internal sealed class SearchTeacherCoursesHandler : IQueryHandler<SearchTeacherCoursesQuery, SearchTeacherCoursesQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public SearchTeacherCoursesHandler(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<SearchTeacherCoursesQueryResponse> ExecuteAsync(SearchTeacherCoursesQuery query, CancellationToken cancellationToken)
    {
        var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var teacherCourseIds = await unitOfWork.ReadOnlyCourseTeacherRepository.ListCourseIdAsync(query.TeacherId, cancellationToken);
        if (teacherCourseIds.Count == 0) { return new SearchTeacherCoursesQueryResponse { CourseInfos = [] }; }

        var courses = await GetCoursesAsync(teacherCourseIds, query, unitOfWork, cancellationToken);
        var courseIds = courses.ToArrayBy(course => course.Id);
        var homeworkCountByCourseId = await GetHomeworkCountByCourseIdAsync(courseIds, unitOfWork, cancellationToken);
        var studentCountByCourseId = await GetStudentCountByCourseIdAsync(courseIds, unitOfWork, cancellationToken);

        return new SearchTeacherCoursesQueryResponse
        {
            CourseInfos = courses.ToArrayBy(
                course => new CourseQueryResponseItem
                {
                    Course = course,
                    StudentCount = studentCountByCourseId[course.Id],
                    HomeworkCount = homeworkCountByCourseId[course.Id]
                })
        };
    }

    private static async Task<IReadOnlyCollection<Course>> GetCoursesAsync(
        IReadOnlyCollection<CourseId> courseIds,
        SearchTeacherCoursesQuery query,
        ICommonReadOnlyUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var courseFilter = new CourseFilter
        {
            CourseIds = courseIds,
            CourseStatuses = query.Filter.CourseStatuses
        };
        var courses = await unitOfWork.ReadOnlyCourseRepository.ListAsync(courseFilter, query.PaginationInfo, cancellationToken);

        return courses
            .Where(course => course.Status != CourseStatus.Deleted)
            .ToArray();
    }

    private static async Task<Dictionary<CourseId, int>> GetHomeworkCountByCourseIdAsync(
        IReadOnlyCollection<CourseId> courseIds,
        ICommonReadOnlyUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var filter = HomeworkFilter.Empty() with { CourseIds = courseIds };
        var homeworks = await unitOfWork.ReadOnlyHomeworkRepository.ListAsync(filter, cancellationToken);

        return homeworks.ToHomeworkCountByCourseId(courseIds);
    }

    private static async Task<Dictionary<CourseId, int>> GetStudentCountByCourseIdAsync(
        IReadOnlyCollection<CourseId> courseIds,
        ICommonReadOnlyUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var filter = GroupFilter.Empty() with { CourseIds = courseIds };
        var groups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(filter, cancellationToken);

        return groups.ToStudentCountByCourseId(courseIds);
    }
}
