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

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.SearchCourses;

internal sealed class SearchCoursesHandler : IQueryHandler<SearchCoursesQuery, SearchCoursesQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public SearchCoursesHandler(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<SearchCoursesQueryResponse> ExecuteAsync(SearchCoursesQuery query, CancellationToken cancellationToken)
    {
        var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var courses = await GetCoursesAsync(query, unitOfWork, cancellationToken);
        if (courses.Count == 0) { return new SearchCoursesQueryResponse { CourseInfos = [] }; }

        var courseIds = courses.ToArrayBy(course => course.Id);
        var homeworkCountByCourseId = await GetHomeworkCountByCourseIdAsync(courseIds, unitOfWork, cancellationToken);
        var studentCountByCourseId = await GetStudentCountByCourseIdAsync(courseIds, unitOfWork, cancellationToken);

        return new SearchCoursesQueryResponse
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

    private static async Task<IReadOnlyCollection<Course>> GetCoursesAsync(
        SearchCoursesQuery query,
        ICommonReadOnlyUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var filter = CourseFilter.Empty() with { CourseStatuses = query.Filter.CourseStatuses };
        return await unitOfWork.ReadOnlyCourseRepository.ListAsync(filter, query.PaginationInfo, cancellationToken);
    }

    private static async Task<Dictionary<CourseId, int>> GetHomeworkCountByCourseIdAsync(
        IReadOnlyCollection<CourseId> courseIds,
        ICommonReadOnlyUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var filter = HomeworkFilter.Empty() with { CourseIds = courseIds };
        var courseHomeworkCounts = await unitOfWork.ReadOnlyHomeworkRepository.ListCourseHomeworkCountAsync(filter, cancellationToken);

        return courseHomeworkCounts.ToDictionary(
            courseHomeworkCount => courseHomeworkCount.CourseId,
            courseHomeworkCount => courseHomeworkCount.HomeworkCount);
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
