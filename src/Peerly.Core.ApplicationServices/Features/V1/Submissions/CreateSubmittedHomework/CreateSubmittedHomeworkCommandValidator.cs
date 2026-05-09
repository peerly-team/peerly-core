using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomework;

internal sealed class CreateSubmittedHomeworkCommandValidator : ICommandValidator<CreateSubmittedHomeworkCommand, CreateSubmittedHomeworkCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IClock _clock;

    public CreateSubmittedHomeworkCommandValidator(ICommonUnitOfWorkFactory unitOfWorkFactory, IClock clock)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _clock = clock;
    }

    public async Task<CommandValidationResult> ValidateAsync(
        CreateSubmittedHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(command.HomeworkId, cancellationToken);
        if (homework is null || homework.Status is HomeworkStatus.Draft)
        {
            return OtherError.NotFound(HomeworkErrors.HomeworkNotFound);
        }

        if (homework.GroupId is { } groupId)
        {
            var groupStudent = new GroupStudent { GroupId = groupId, StudentId = command.StudentId };
            var isGroupStudentExists = await unitOfWork.ReadOnlyGroupStudentRepository.ExistsAsync(groupStudent, cancellationToken);
            if (!isGroupStudentExists)
            {
                return OtherError.NotFound(HomeworkErrors.HomeworkNotFound);
            }
        }
        else
        {
            var courseStudent = new CourseStudent { CourseId = homework.CourseId, StudentId = command.StudentId };
            var isCourseStudentExists = await unitOfWork.ReadOnlyGroupStudentRepository.ExistsAsync(courseStudent, cancellationToken);
            if (!isCourseStudentExists)
            {
                return OtherError.NotFound(HomeworkErrors.HomeworkNotFound);
            }
        }

        if (homework.Status is not HomeworkStatus.Published)
        {
            return OtherError.Conflict(HomeworkErrors.HomeworkNotAcceptingSubmissions);
        }

        if (_clock.GetCurrentTime() >= homework.Deadline)
        {
            return OtherError.Conflict(HomeworkErrors.HomeworkDeadlinePassed);
        }

        var homeworkStudent = new HomeworkStudent { HomeworkId = command.HomeworkId, StudentId = command.StudentId };
        var submittedHomeworkId = await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetSubmittedHomeworkIdAsync(homeworkStudent, cancellationToken);
        if (submittedHomeworkId is not null)
        {
            return OtherError.Conflict(SubmittedHomeworkErrors.SubmittedHomeworkAlreadySubmitted);
        }

        return CommandValidationResult.Ok();
    }
}
