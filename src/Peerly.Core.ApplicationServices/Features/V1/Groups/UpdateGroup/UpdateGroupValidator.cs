using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Groups.UpdateGroup.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.UpdateGroup;

internal sealed class UpdateGroupValidator : IUpdateGroupValidator
{
    public async Task<OtherError?> ValidateAsync(ICommonUnitOfWork unitOfWork, UpdateGroupCommand command, CancellationToken cancellationToken)
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

        return null;
    }
}
