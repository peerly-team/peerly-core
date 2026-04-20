using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Checkers.CourseAccess.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetStudentHomework;

internal sealed class GetStudentHomeworkHandler : IQueryHandler<GetStudentHomeworkQuery, GetStudentHomeworkQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IStudentCourseAccessChecker _studentCourseAccessChecker;

    public GetStudentHomeworkHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IStudentCourseAccessChecker studentCourseAccessChecker)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _studentCourseAccessChecker = studentCourseAccessChecker;
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
        var submittedHomeworkId =
            await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetSubmittedHomeworkIdAsync(homeworkStudent, cancellationToken);
        var files = await unitOfWork.ReadOnlyHomeworkFileRepository.ListFilesAsync(homework.Id, cancellationToken);

        return new GetStudentHomeworkQueryResponse
        {
            Homework = homework,
            SubmittedHomeworkId = submittedHomeworkId,
            Files = files
        };
    }

    private async Task EnsureStudentHasAccessAsync(
        ICommonReadOnlyUnitOfWork unitOfWork,
        GetStudentHomeworkQuery query,
        Homework homework,
        CancellationToken cancellationToken)
    {
        var courseStudent = query.ToCourseStudent(homework.CourseId);
        if (!await _studentCourseAccessChecker.RunAsync(courseStudent, cancellationToken))
        {
            throw new NotFoundException();
        }

        if (homework.GroupId is not { } homeworkGroupId)
        {
            return;
        }

        var groupStudentFilter = new GroupStudentFilter
        {
            GroupIds = [homeworkGroupId],
            StudentIds = [query.StudentId]
        };
        var groupStudents = await unitOfWork.ReadOnlyGroupStudentRepository.ListAsync(groupStudentFilter, cancellationToken);
        if (groupStudents.Count == 0)
        {
            throw new NotFoundException();
        }
    }
}
