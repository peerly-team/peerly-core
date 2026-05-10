using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomework;

internal sealed class DeleteSubmittedHomeworkValidator : ICommandValidator<DeleteSubmittedHomeworkCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IClock _clock;

    public DeleteSubmittedHomeworkValidator(ICommonUnitOfWorkFactory unitOfWorkFactory, IClock clock)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _clock = clock;
    }

    public async Task<CommandValidationResult> ValidateAsync(DeleteSubmittedHomeworkCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var submittedHomework = await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, cancellationToken);
        if (submittedHomework is null || submittedHomework.StudentId != command.StudentId)
        {
            return OtherError.NotFound(SubmittedHomeworkErrors.SubmittedHomeworkNotFound);
        }

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, cancellationToken);
        if (homework is null)
        {
            return OtherError.NotFound(HomeworkErrors.HomeworkNotFound);
        }

        if (homework.Status is not HomeworkStatus.Published || _clock.GetCurrentTime() >= homework.Deadline)
        {
            return ValidationError.From(HomeworkErrors.HomeworkNotAcceptingSubmissions);
        }

        return CommandValidationResult.Ok();
    }
}
