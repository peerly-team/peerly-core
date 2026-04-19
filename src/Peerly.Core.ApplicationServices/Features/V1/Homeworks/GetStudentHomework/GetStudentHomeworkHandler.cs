using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
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
        if (homework is null || homework.Status is HomeworkStatus.Draft)
        {
            throw new NotFoundException();
        }

        await EnsureStudentHasAccessAsync(unitOfWork, query, homework, cancellationToken);

        var homeworkStudent = query.ToHomeworkStudent(homework.Id);
        var submittedHomeworkId = await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetSubmittedHomeworkIdAsync(homeworkStudent, cancellationToken);
        var files = await unitOfWork.ReadOnlyHomeworkFileRepository.ListFilesAsync(homework.Id, cancellationToken);

        return new GetStudentHomeworkQueryResponse
        {
            Homework = homework,
            SubmittedHomeworkId = submittedHomeworkId,
            Files = files
        };
    }

    private static async Task EnsureStudentHasAccessAsync(
        ICommonReadOnlyUnitOfWork unitOfWork,
        GetStudentHomeworkQuery query,
        Homework homework,
        CancellationToken cancellationToken)
    {
        var courseGroupFilter = query.ToCourseGroupFilter(homework.CourseId);
        var courseGroups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(courseGroupFilter, cancellationToken);
        if (courseGroups.Count == 0)
        {
            throw new NotFoundException();
        }

        var groupStudentFilter = query.ToGroupStudentFilter(courseGroups.ToArrayBy(group => group.Id));
        var studentGroups = await unitOfWork.ReadOnlyGroupStudentRepository.ListAsync(groupStudentFilter, cancellationToken);
        if (studentGroups.Count == 0)
        {
            throw new NotFoundException();
        }

        if (homework.GroupId is { } homeworkGroupId && studentGroups.All(groupStudent => groupStudent.GroupId != homeworkGroupId))
        {
            throw new NotFoundException();
        }
    }
}
