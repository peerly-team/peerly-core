using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Checkers.CourseAccess.Abstractions;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Checkers.CourseAccess;

internal sealed class StudentCourseAccessChecker : IStudentCourseAccessChecker
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public StudentCourseAccessChecker(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<bool> RunAsync(CourseStudent courseStudent, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var courseGroupFilter = GroupFilter.Empty() with { CourseIds = [courseStudent.CourseId] };
        var courseGroups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(courseGroupFilter, cancellationToken);
        if (courseGroups.Count == 0)
        {
            return false;
        }

        var groupStudentFilter = new GroupStudentFilter
        {
            GroupIds = courseGroups.Select(group => group.Id).ToArray(),
            StudentIds = [courseStudent.StudentId]
        };
        var groupStudents = await unitOfWork.ReadOnlyGroupStudentRepository.ListAsync(groupStudentFilter, cancellationToken);
        return groupStudents.Count > 0;
    }
}
