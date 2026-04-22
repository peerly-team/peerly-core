using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomeworkFile.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomeworkFile;

internal sealed class CreateHomeworkFileValidator : ICreateHomeworkFileValidator
{
    public async Task<OtherError?> ValidateAsync(
        ICommonUnitOfWork unitOfWork,
        CreateHomeworkFileCommand command,
        CancellationToken cancellationToken)
    {
        var homework = await unitOfWork.HomeworkRepository.GetAsync(command.HomeworkId, cancellationToken);
        if (homework is null)
        {
            return OtherError.NotFound();
        }

        var courseTeacher = command.ToCourseTeacher(homework.CourseId);
        if (!await unitOfWork.CourseTeacherRepository.ExistsAsync(courseTeacher, cancellationToken))
        {
            return OtherError.PermissionDenied();
        }

        return null;
    }
}
