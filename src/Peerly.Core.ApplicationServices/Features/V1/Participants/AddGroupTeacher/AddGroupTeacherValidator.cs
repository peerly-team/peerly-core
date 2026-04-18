using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupTeacher.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupTeacher;

internal sealed class AddGroupTeacherValidator : IAddGroupTeacherValidator
{
    public async Task<OtherError?> ValidateAsync(ICommonUnitOfWork unitOfWork, AddGroupTeacherCommand command, CancellationToken cancellationToken)
    {
        var groupFilter = command.ToGroupFilter();
        var group = (await unitOfWork.GroupRepository.ListAsync(groupFilter, cancellationToken)).SingleOrDefault();
        if (group is null)
        {
            return OtherError.NotFound();
        }

        var teacherFilter = command.ToTeacherFilter();
        var teachers = await unitOfWork.TeacherRepository.ListAsync(teacherFilter, cancellationToken);
        if (teachers.Count != teacherFilter.TeacherIds.Count)
        {
            return OtherError.NotFound();
        }

        var courseTeacherExistsItem = command.ToCourseTeacherExistsItem(group.CourseId);
        var actorIsCourseTeacher = await unitOfWork.CourseTeacherRepository.ExistsAsync(courseTeacherExistsItem, cancellationToken);
        if (!actorIsCourseTeacher)
        {
            return OtherError.PermissionDenied();
        }

        var groupTeacherFilter = command.ToGroupTeacherFilter();
        var existing = await unitOfWork.GroupTeacherRepository.ListAsync(groupTeacherFilter, cancellationToken);
        if (existing.Count != 0)
        {
            return OtherError.Conflict();
        }

        return null;
    }
}
