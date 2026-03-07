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
        var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var groups = await GetGroupsAsync(query.StudentId, unitOfWork, cancellationToken);
        if (groups.Count == 0) { return new SearchStudentCoursesQueryResponse { CourseInfos = [] }; }

        var courses = await GetCoursesAsync(groups, query, unitOfWork, cancellationToken);
        var courseIds = courses.ToArrayBy(course => course.Id);
        var homeworkCountByCourseId = await GetHomeworkCountByCourseIdAsync(courseIds, unitOfWork, cancellationToken);
        var studentCountByCourseId = groups.ToStudentCountByCourseId(courseIds);

        return new SearchStudentCoursesQueryResponse
        {
            CourseInfos = courses.ToArrayBy(
                course => new SearchCoursesQueryResponseItem
                {
                    Course = course,
                    StudentCount = studentCountByCourseId[course.Id],
                    HomeworkCount = homeworkCountByCourseId[course.Id]
                })
        };
    }

    private static async Task<IReadOnlyCollection<Group>> GetGroupsAsync(
        StudentId studentId,
        ICommonReadOnlyUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var groupIds = await unitOfWork.ReadOnlyGroupStudentRepository.ListGroupIdAsync(studentId, cancellationToken);
        if (groupIds.Count == 0) { return []; }

        var groupFilter = GroupFilter.Empty() with { GroupIds = groupIds };
        return await unitOfWork.ReadOnlyGroupRepository.ListAsync(groupFilter, cancellationToken);
    }

    private static async Task<IReadOnlyCollection<Course>> GetCoursesAsync(
        IReadOnlyCollection<Group> groups,
        SearchStudentCoursesQuery query,
        ICommonReadOnlyUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var courseFilter = new CourseFilter
        {
            CourseIds = groups.ToArrayBy(group => group.CourseId),
            CourseStatuses = query.Filter.CourseStatuses
        };
        return await unitOfWork.ReadOnlyCourseRepository.ListAsync(courseFilter, query.PaginationInfo, cancellationToken);
    }

    private static async Task<Dictionary<CourseId, int>> GetHomeworkCountByCourseIdAsync(
        IReadOnlyCollection<CourseId> courseIds,
        ICommonReadOnlyUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var filter = new HomeworkFilter
        {
            CourseIds = courseIds,
            HomeworkStatuses = [HomeworkStatus.Published, HomeworkStatus.Review, HomeworkStatus.Closed]
        };
        var courseHomeworkCounts = await unitOfWork.ReadOnlyHomeworkRepository.ListCourseHomeworkCountAsync(filter, cancellationToken);

        return courseHomeworkCounts.ToDictionary(
            courseHomeworkCount => courseHomeworkCount.CourseId,
            courseHomeworkCount => courseHomeworkCount.HomeworkCount);
    }
}
