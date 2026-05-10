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

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;

internal sealed class CreateSubmittedHomeworkFileValidator : ICommandValidator<CreateSubmittedHomeworkFileCommand, CreateSubmittedHomeworkFileCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IClock _clock;

    public CreateSubmittedHomeworkFileValidator(ICommonUnitOfWorkFactory unitOfWorkFactory, IClock clock)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _clock = clock;
    }

    public async Task<CommandValidationResult> ValidateAsync(CreateSubmittedHomeworkFileCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var submittedHomework = await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, cancellationToken);
        if (submittedHomework is null || submittedHomework.StudentId != command.StudentId)
        {
            return OtherError.NotFound(SubmittedHomeworkErrors.SubmittedHomeworkNotFound);
        }

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, cancellationToken);
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

        if (homework.Status is not HomeworkStatus.Published || _clock.GetCurrentTime() >= homework.Deadline)
        {
            return OtherError.Conflict(HomeworkErrors.HomeworkNotAcceptingSubmissions);
        }

        return CommandValidationResult.Ok();
    }
}
