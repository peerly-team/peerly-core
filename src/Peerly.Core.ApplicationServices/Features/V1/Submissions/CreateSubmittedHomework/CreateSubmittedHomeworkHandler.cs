using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.ApplicationServices.Validators.HomeworkSubmissionAccess.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomework;

internal sealed class CreateSubmittedHomeworkHandler :
    ICommandHandler<CreateSubmittedHomeworkCommand, CreateSubmittedHomeworkCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IHomeworkSubmissionAccessValidator _homeworkSubmissionAccessValidator;
    private readonly IClock _clock;

    public CreateSubmittedHomeworkHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IClock clock,
        IHomeworkSubmissionAccessValidator homeworkSubmissionAccessValidator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _clock = clock;
        _homeworkSubmissionAccessValidator = homeworkSubmissionAccessValidator;
    }

    public async Task<CommandResponse<CreateSubmittedHomeworkCommandResponse>> ExecuteAsync(
        CreateSubmittedHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var validationError = await _homeworkSubmissionAccessValidator.RunAsync(command.HomeworkId, command.StudentId, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var submittedHomeworkAddItem = command.ToSubmittedHomeworkAddItem(_clock.GetCurrentTime());
        var submittedHomeworkId = await unitOfWork.SubmittedHomeworkRepository.AddAsync(submittedHomeworkAddItem, cancellationToken);

        return new CreateSubmittedHomeworkCommandResponse
        {
            SubmittedHomeworkId = submittedHomeworkId
        };
    }
}
