using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomework;

internal sealed class CreateSubmittedHomeworkHandler : ICommandHandler<CreateSubmittedHomeworkCommand, CreateSubmittedHomeworkCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ICommandValidator<CreateSubmittedHomeworkCommand, CreateSubmittedHomeworkCommandResponse> _validator;
    private readonly IClock _clock;

    public CreateSubmittedHomeworkHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IClock clock,
        ICommandValidator<CreateSubmittedHomeworkCommand, CreateSubmittedHomeworkCommandResponse> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _clock = clock;
        _validator = validator;
    }

    public async Task<CommandResponse<CreateSubmittedHomeworkCommandResponse>> ExecuteAsync(
        CreateSubmittedHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var submittedHomeworkAddItem = command.ToSubmittedHomeworkAddItem(_clock.GetCurrentTime());
        var submittedHomeworkId = await unitOfWork.SubmittedHomeworkRepository.AddAsync(submittedHomeworkAddItem, cancellationToken);

        return new CreateSubmittedHomeworkCommandResponse
        {
            SubmittedHomeworkId = submittedHomeworkId
        };
    }
}
