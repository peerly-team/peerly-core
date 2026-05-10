using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomework;

internal sealed class DeleteSubmittedHomeworkHandler : ICommandHandler<DeleteSubmittedHomeworkCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ICommandValidator<DeleteSubmittedHomeworkCommand, Success> _validator;

    public DeleteSubmittedHomeworkHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        ICommandValidator<DeleteSubmittedHomeworkCommand, Success> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(DeleteSubmittedHomeworkCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        await unitOfWork.SubmittedHomeworkFileRepository.DeleteBySubmittedHomeworkAsync(command.SubmittedHomeworkId, cancellationToken);
        await unitOfWork.SubmittedHomeworkRepository.DeleteAsync(command.SubmittedHomeworkId, cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new Success();
    }
}
