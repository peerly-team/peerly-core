using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetTeacherHomework;

internal sealed class GetTeacherHomeworkHandler : IQueryHandler<GetTeacherHomeworkQuery, GetTeacherHomeworkQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public GetTeacherHomeworkHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<GetTeacherHomeworkQueryResponse> ExecuteAsync(
        GetTeacherHomeworkQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(query.HomeworkId, cancellationToken)
                       ?? throw new NotFoundException();

        await EnsureTeacherHasAccessAsync(unitOfWork, query, homework, cancellationToken);

        var submittedFilter = query.ToSubmittedHomeworkFilter(homework.Id);
        var submittedHomeworks =
            await unitOfWork.ReadOnlySubmittedHomeworkRepository.ListSubmittedHomeworkStudentAsync(submittedFilter, cancellationToken);
        var totalStudentsCount = await GetTotalStudentsCountAsync(unitOfWork, query, homework, cancellationToken);
        var files = await unitOfWork.ReadOnlyHomeworkFileRepository.ListFilesAsync(homework.Id, cancellationToken);

        return new GetTeacherHomeworkQueryResponse
        {
            Homework = homework,
            SubmittedCount = submittedHomeworks.Count,
            TotalStudentsCount = totalStudentsCount,
            Files = files
        };
    }

    private static async Task EnsureTeacherHasAccessAsync(
        ICommonReadOnlyUnitOfWork unitOfWork,
        GetTeacherHomeworkQuery query,
        Homework homework,
        CancellationToken cancellationToken)
    {
        var courseTeacher = query.ToCourseTeacher(homework.CourseId);
        if (await unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, cancellationToken))
        {
            return;
        }

        var courseGroupFilter = query.ToCourseGroupFilter(homework.CourseId);
        var courseGroups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(courseGroupFilter, cancellationToken);
        if (courseGroups.Count == 0)
        {
            throw new NotFoundException();
        }

        var groupTeacherFilter = query.ToGroupTeacherFilter();
        var teacherGroups = await unitOfWork.ReadOnlyGroupTeacherRepository.ListAsync(groupTeacherFilter, cancellationToken);
        var courseGroupIds = courseGroups.Select(group => group.Id).ToHashSet();
        if (!teacherGroups.Any(groupTeacher => courseGroupIds.Contains(groupTeacher.GroupId)))
        {
            throw new NotFoundException();
        }
    }

    private static async Task<int> GetTotalStudentsCountAsync(
        ICommonReadOnlyUnitOfWork unitOfWork,
        GetTeacherHomeworkQuery query,
        Homework homework,
        CancellationToken cancellationToken)
    {
        var filter = homework.GroupId is { } homeworkGroupId
            ? query.ToSingleGroupFilter(homeworkGroupId)
            : query.ToCourseGroupFilter(homework.CourseId);

        var groups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(filter, cancellationToken);
        return groups.Sum(group => group.StudentCount);
    }
}
