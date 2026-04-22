using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Groups.DeleteGroup.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.DeleteGroup;

internal sealed class DeleteGroupValidator : IDeleteGroupValidator
{
    public async Task<OtherError?> ValidateAsync(ICommonUnitOfWork unitOfWork, DeleteGroupCommand command, CancellationToken cancellationToken)
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

        var homeworkFilter = command.ToHomeworkFilter();
        var homeworks = await unitOfWork.HomeworkRepository.ListAsync(homeworkFilter, cancellationToken);
        if (homeworks.Count != 0)
        {
            return OtherError.Conflict();
        }

        return null;
    }
}
