using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchStudentHomeworks;

internal sealed class SearchStudentHomeworksHandler : IQueryHandler<SearchStudentHomeworksQuery, SearchStudentHomeworksQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public SearchStudentHomeworksHandler(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<SearchStudentHomeworksQueryResponse> ExecuteAsync(
        SearchStudentHomeworksQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var courseIds = await unitOfWork.ReadOnlyGroupRepository.ListCourseIdsAsync(query.StudentId, cancellationToken);
        if (courseIds.Count == 0)
            return new SearchStudentHomeworksQueryResponse { StudentHomeworks = [] };

        var groupIds = await unitOfWork.ReadOnlyGroupRepository.ListGroupIdsAsync(query.StudentId, cancellationToken);

        var filter = new StudentHomeworkSearchFilter
        {
            StudentId = query.StudentId,
            CourseIds = courseIds,
            GroupIds = groupIds,
            HomeworkStatuses = query.Filter.ResolveStatuses()
        };
        var homeworks = await unitOfWork.ReadOnlyHomeworkRepository.SearchStudentHomeworkInfosAsync(filter, query.PaginationInfo, cancellationToken);

        return new SearchStudentHomeworksQueryResponse { StudentHomeworks = homeworks };
    }
}
