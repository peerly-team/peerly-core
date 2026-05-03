using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Models.Courses;

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
            return new SearchStudentCoursesQueryResponse { Courses = [] };

        var courseFilter = new CourseFilter
        {
            CourseIds = courseIds,
            CourseStatuses = query.Filter.CourseStatuses
        };
        var courses = await unitOfWork.ReadOnlyCourseRepository.ListAsync(courseFilter, query.PaginationInfo, cancellationToken);

        return new SearchStudentCoursesQueryResponse
        {
            Courses = courses
                .Where(course => course.Status != CourseStatus.Deleted)
                .ToArray()
        };
    }
}
