using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Models.Courses;
using Peerly.Core.Pagination;
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
        var filter = new CourseFilter
        {
            CourseStatuses = query.Filter.CourseStatuses
        };
        var paginationInfo = new PaginationInfo
        {
            Offset = query.PaginationInfo.Offset,
            PageSize = query.PaginationInfo.PageSize
        };

        return await unitOfWork.ReadOnlyCourseRepository.ListAsync(filter, paginationInfo, cancellationToken);
    }

    private static async Task<Dictionary<long, int>> GetHomeworkCountByCourseIdAsync(
        IReadOnlyCollection<long> courseIds,
        ICommonReadOnlyUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var courseHomeworkCounts = await unitOfWork.ReadOnlyHomeworkRepository.ListCourseHomeworkCountsAsync(courseIds, cancellationToken);
        return courseHomeworkCounts.ToDictionary(
            courseHomeworkCount => courseHomeworkCount.CourseId,
            courseHomeworkCount => courseHomeworkCount.HomeworkCount);
    }

    private static async Task<Dictionary<long, int>> GetStudentCountByCourseIdAsync(
        IReadOnlyCollection<long> courseIds,
        ICommonReadOnlyUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var courseGroupStudentCounts = await unitOfWork.ReadOnlyGroupRepository.ListCourseGroupStudentCountAsync(courseIds, cancellationToken);
        return courseGroupStudentCounts
            .GroupBy(x => x.CourseId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => x.StudentCount));
    }
}
