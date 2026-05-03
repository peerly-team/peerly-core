using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Models.Courses;

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
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var courseIds = await unitOfWork.ReadOnlyCourseTeacherRepository.ListCourseIdAsync(query.TeacherId, cancellationToken);
        var groupCourseIds = await unitOfWork.ReadOnlyGroupRepository.ListCourseIdAsync(query.TeacherId, cancellationToken);
        var generalCourseIds = courseIds
            .Concat(groupCourseIds)
            .ToArray();

        if (generalCourseIds.Length == 0)
            return new SearchTeacherCoursesQueryResponse { Courses = [] };

        var courseFilter = new CourseFilter
        {
            CourseIds = generalCourseIds,
            CourseStatuses = query.Filter.CourseStatuses
        };
        var courses = await unitOfWork.ReadOnlyCourseRepository.ListAsync(courseFilter, query.PaginationInfo, cancellationToken);

        return new SearchTeacherCoursesQueryResponse
        {
            Courses = courses
                .Where(course => course.Status != CourseStatus.Deleted)
                .ToArray()
        };
    }
}
