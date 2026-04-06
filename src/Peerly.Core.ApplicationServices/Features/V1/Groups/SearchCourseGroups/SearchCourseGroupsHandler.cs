using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.SearchCourseGroups;

internal sealed class SearchCourseGroupsHandler : IQueryHandler<SearchCourseGroupsQuery, SearchCourseGroupsQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public SearchCourseGroupsHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<SearchCourseGroupsQueryResponse> ExecuteAsync(
        SearchCourseGroupsQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var courseTeacherExistsItem = new CourseTeacherExistsItem
        {
            CourseId = query.CourseId,
            TeacherId = query.TeacherId
        };
        var isCourseTeacher = await unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacherExistsItem, cancellationToken);
        if (!isCourseTeacher)
        {
            throw new NotFoundException();
        }

        var groupFilter = GroupFilter.Empty() with { CourseIds = [query.CourseId] };
        var groups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(groupFilter, cancellationToken);

        return new SearchCourseGroupsQueryResponse
        {
            Groups = groups
        };
    }
}
