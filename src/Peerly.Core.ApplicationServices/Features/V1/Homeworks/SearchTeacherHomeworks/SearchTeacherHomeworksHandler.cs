using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchTeacherHomeworks;

internal sealed class SearchTeacherHomeworksHandler : IQueryHandler<SearchTeacherHomeworksQuery, SearchTeacherHomeworksQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public SearchTeacherHomeworksHandler(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<SearchTeacherHomeworksQueryResponse> ExecuteAsync(SearchTeacherHomeworksQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var courseIds = await unitOfWork.ReadOnlyCourseTeacherRepository.ListCourseIdsAsync(query.TeacherId, cancellationToken);
        var groupIds = await unitOfWork.ReadOnlyGroupTeacherRepository.ListGroupIdsAsync(query.TeacherId, cancellationToken);

        if (courseIds.Count == 0 && groupIds.Count == 0)
            return new SearchTeacherHomeworksQueryResponse { TeacherHomeworks = [] };

        var filter = new TeacherHomeworkSearchFilter
        {
            CourseIds = courseIds,
            GroupIds = groupIds,
            HomeworkStatuses = query.Filter.HomeworkStatuses
        };
        var homeworks = await unitOfWork.ReadOnlyHomeworkRepository.SearchTeacherHomeworkInfosAsync(filter, query.PaginationInfo, cancellationToken);

        return new SearchTeacherHomeworksQueryResponse { TeacherHomeworks = homeworks };
    }
}
