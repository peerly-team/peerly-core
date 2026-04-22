using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Checkers.CourseAccess.Abstractions;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Checkers.CourseAccess;

internal sealed class TeacherCourseAccessChecker : ITeacherCourseAccessChecker
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public TeacherCourseAccessChecker(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<bool> RunAsync(CourseTeacher courseTeacher, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        if (await unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, cancellationToken))
        {
            return true;
        }

        var courseGroupFilter = GroupFilter.Empty() with { CourseIds = [courseTeacher.CourseId] };
        var courseGroups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(courseGroupFilter, cancellationToken);
        if (courseGroups.Count == 0)
        {
            return false;
        }

        var groupTeacherFilter = GroupTeacherFilter.Empty() with { TeacherIds = [courseTeacher.TeacherId] };
        var teacherGroups = await unitOfWork.ReadOnlyGroupTeacherRepository.ListAsync(groupTeacherFilter, cancellationToken);

        var courseGroupIds = courseGroups.Select(group => group.Id).ToHashSet();
        return teacherGroups.Any(groupTeacher => courseGroupIds.Contains(groupTeacher.GroupId));
    }
}
