using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateCourseHomework.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateCourseHomework;

internal sealed class CreateCourseHomeworkValidator : ICreateCourseHomeworkValidator
{
    public async Task<OtherError?> ValidateAsync(ICommonUnitOfWork unitOfWork, CreateCourseHomeworkCommand command, CancellationToken cancellationToken)
    {
        var courseTeacher = command.ToCourseTeacher();
        if (!await unitOfWork.CourseTeacherRepository.ExistsAsync(courseTeacher, cancellationToken))
        {
            return OtherError.PermissionDenied();
        }

        var course = await unitOfWork.CourseRepository.GetAsync(command.CourseId, cancellationToken);
        if (course is null)
        {
            return OtherError.NotFound();
        }

        return null;
    }
}
