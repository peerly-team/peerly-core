using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ListStudentCourseHomeworks;

internal sealed class ListStudentCourseHomeworksHandler : IQueryHandler<ListStudentCourseHomeworksQuery, ListStudentCourseHomeworksQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public ListStudentCourseHomeworksHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<ListStudentCourseHomeworksQueryResponse> ExecuteAsync(
        ListStudentCourseHomeworksQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var groupFilter = query.ToGroupFilter();
        var groups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(groupFilter, cancellationToken);
        if (groups.Count == 0)
        {
            throw new NotFoundException();
        }

        var groupStudentFilter = query.ToGroupStudentFilter(groups.ToArrayBy(group => group.Id));
        var studentGroups = await unitOfWork.ReadOnlyGroupStudentRepository.ListAsync(groupStudentFilter, cancellationToken);
        if (studentGroups.Count == 0)
        {
            throw new NotFoundException();
        }

        var groupHomeworkFilter = query.ToGroupHomeworkFilter(studentGroups.ToArrayBy(groupStudent => groupStudent.GroupId));
        var groupHomeworks = await unitOfWork.ReadOnlyHomeworkRepository.ListAsync(groupHomeworkFilter, cancellationToken);

        var courseHomeworkFilter = query.ToCourseHomeworkFilter();
        var allCourseHomeworks = await unitOfWork.ReadOnlyHomeworkRepository.ListAsync(courseHomeworkFilter, cancellationToken);
        var courseLevelHomeworks = allCourseHomeworks.Where(homework => homework.GroupId is null);

        var homeworks = groupHomeworks
            .Concat(courseLevelHomeworks)
            .DistinctBy(homework => homework.Id)
            .ToArray();

        return new ListStudentCourseHomeworksQueryResponse
        {
            Homeworks = homeworks
        };
    }
}
