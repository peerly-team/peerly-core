using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework;

internal sealed class CreateGroupHomeworkValidator : ICreateGroupHomeworkValidator
{
    public async Task<OtherError?> ValidateAsync(ICommonUnitOfWork unitOfWork, CreateGroupHomeworkCommand command, CourseId courseId, CancellationToken cancellationToken)
    {
        var courseTeacher = command.ToCourseTeacher(courseId);
        if (!await unitOfWork.CourseTeacherRepository.ExistsAsync(courseTeacher, cancellationToken))
        {
            return OtherError.PermissionDenied();
        }

        return null;
    }
}
