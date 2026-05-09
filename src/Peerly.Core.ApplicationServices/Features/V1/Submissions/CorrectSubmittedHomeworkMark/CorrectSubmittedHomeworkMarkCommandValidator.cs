using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CorrectSubmittedHomeworkMark;

internal sealed class CorrectSubmittedHomeworkMarkCommandValidator : ICommandValidator<CorrectSubmittedHomeworkMarkCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public CorrectSubmittedHomeworkMarkCommandValidator(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<CommandValidationResult> ValidateAsync(CorrectSubmittedHomeworkMarkCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var submittedHomework = await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, cancellationToken);
        if (submittedHomework is null)
        {
            return OtherError.NotFound(SubmittedHomeworkErrors.SubmittedHomeworkNotFound);
        }

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, cancellationToken);
        if (homework is null)
        {
            return OtherError.NotFound(HomeworkErrors.HomeworkNotFound);
        }

        var courseTeacher = new CourseTeacher { CourseId = homework.CourseId, TeacherId = command.TeacherId };
        var isCourseTeacher = await unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, cancellationToken);
        if (!isCourseTeacher)
        {
            var isGroupTeacher = await unitOfWork.ReadOnlyGroupTeacherRepository.ExistsAsync(courseTeacher, cancellationToken);
            if (!isGroupTeacher)
            {
                return OtherError.PermissionDenied();
            }
        }

        if (homework.Status is not HomeworkStatus.Confirmation)
        {
            return OtherError.Conflict(HomeworkErrors.HomeworkNotInConfirmationStatus);
        }

        return CommandValidationResult.Ok();
    }
}
