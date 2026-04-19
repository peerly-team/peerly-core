using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupStudent.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupStudent;

internal sealed class AddGroupStudentValidator : IAddGroupStudentValidator
{
    public async Task<OtherError?> ValidateAsync(ICommonUnitOfWork unitOfWork, AddGroupStudentCommand command, CancellationToken cancellationToken)
    {
        var group = await unitOfWork.GroupRepository.GetAsync(command.GroupId, cancellationToken);
        if (group is null)
        {
            return OtherError.NotFound();
        }

        var courseTeacher = command.ToCourseTeacher(group.CourseId);
        if (!await unitOfWork.CourseTeacherRepository.ExistsAsync(courseTeacher, cancellationToken))
        {
            return OtherError.PermissionDenied();
        }

        var studentFilter = command.ToStudentFilter();
        var students = await unitOfWork.StudentRepository.ListAsync(studentFilter, cancellationToken);
        if (students.Count != studentFilter.StudentIds.Count)
        {
            return OtherError.NotFound();
        }

        var teacherFilter = command.ToTeacherFilter();
        var teachers = await unitOfWork.TeacherRepository.ListAsync(teacherFilter, cancellationToken);
        if (teachers.Count != teacherFilter.TeacherIds.Count)
        {
            return OtherError.NotFound();
        }

        var groupStudentFilter = command.ToGroupStudentFilter();
        var existing = await unitOfWork.GroupStudentRepository.ListAsync(groupStudentFilter, cancellationToken);
        if (existing.Count != 0)
        {
            return OtherError.Conflict();
        }

        return null;
    }
}
