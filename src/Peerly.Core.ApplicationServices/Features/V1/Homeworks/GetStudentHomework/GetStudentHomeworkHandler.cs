using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.Shared.GetHomework;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetStudentHomework;

internal sealed class GetStudentHomeworkHandler : IQueryHandler<GetStudentHomeworkQuery, GetStudentHomeworkQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public GetStudentHomeworkHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<GetStudentHomeworkQueryResponse> ExecuteAsync(
        GetStudentHomeworkQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(query.HomeworkId, cancellationToken);
        if (homework is null)
        {
            throw new NotFoundException();
        }

        if (homework.Status == HomeworkStatus.Draft)
        {
            throw new NotFoundException();
        }

        var groupFilter = GroupFilter.Empty() with { CourseIds = [homework.CourseId] };
        var groups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(groupFilter, cancellationToken);

        var groupStudentFilter = new GroupStudentFilter
        {
            GroupIds = groups.ToArrayBy(g => g.Id),
            StudentIds = [query.StudentId]
        };
        var groupStudents = await unitOfWork.ReadOnlyGroupStudentRepository.ListAsync(groupStudentFilter, cancellationToken);
        if (groupStudents.Count == 0)
        {
            throw new NotFoundException();
        }

        var fileIds = await unitOfWork.ReadOnlyHomeworkFileRepository.ListFileIdsAsync(query.HomeworkId, cancellationToken);
        var files = fileIds.Count > 0
            ? await unitOfWork.ReadOnlyFileRepository.ListByIdsAsync(fileIds, cancellationToken)
            : [];

        return new GetStudentHomeworkQueryResponse
        {
            HomeworkDetail = new HomeworkDetailResponseItem
            {
                Homework = homework,
                Files = files
            }
        };
    }
}
