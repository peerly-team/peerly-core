using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Checkers.CourseAccess.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetTeacherHomework;

internal sealed class GetTeacherHomeworkHandler : IQueryHandler<GetTeacherHomeworkQuery, GetTeacherHomeworkQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ITeacherCourseAccessChecker _teacherCourseAccessChecker;

    public GetTeacherHomeworkHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        ITeacherCourseAccessChecker teacherCourseAccessChecker)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _teacherCourseAccessChecker = teacherCourseAccessChecker;
    }

    public async Task<GetTeacherHomeworkQueryResponse> ExecuteAsync(
        GetTeacherHomeworkQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(query.HomeworkId, cancellationToken)
                       ?? throw new NotFoundException();

        var courseTeacher = query.ToCourseTeacher(homework.CourseId);
        if (!await _teacherCourseAccessChecker.RunAsync(courseTeacher, cancellationToken))
        {
            throw new NotFoundException();
        }

        var submittedFilter = query.ToSubmittedHomeworkFilter(homework.Id);
        var submittedHomeworks =
            await unitOfWork.ReadOnlySubmittedHomeworkRepository.ListSubmittedHomeworkStudentAsync(submittedFilter, cancellationToken);
        var totalStudentsCount = await GetTotalStudentsCountAsync(unitOfWork, homework, cancellationToken);
        var files = await unitOfWork.ReadOnlyHomeworkFileRepository.ListFilesAsync(homework.Id, cancellationToken);

        return new GetTeacherHomeworkQueryResponse
        {
            Homework = homework,
            SubmittedCount = submittedHomeworks.Count,
            TotalStudentsCount = totalStudentsCount,
            Files = files
        };
    }

    private static async Task<int> GetTotalStudentsCountAsync(
        ICommonReadOnlyUnitOfWork unitOfWork,
        Homework homework,
        CancellationToken cancellationToken)
    {
        var filter = homework.GroupId is { } homeworkGroupId
            ? GroupFilter.Empty() with { GroupIds = [homeworkGroupId] }
            : GroupFilter.Empty() with { CourseIds = [homework.CourseId] };

        var groups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(filter, cancellationToken);
        return groups.Sum(group => group.StudentCount);
    }
}
