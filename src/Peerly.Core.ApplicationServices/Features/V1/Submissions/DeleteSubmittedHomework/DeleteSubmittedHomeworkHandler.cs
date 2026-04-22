using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomework.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomework;

internal sealed class DeleteSubmittedHomeworkHandler : ICommandHandler<DeleteSubmittedHomeworkCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IDeleteSubmittedHomeworkValidator _validator;

    public DeleteSubmittedHomeworkHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IDeleteSubmittedHomeworkValidator validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        DeleteSubmittedHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var validationError = await _validator.ValidateAsync(unitOfWork, command, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        await unitOfWork.SubmittedHomeworkFileRepository.DeleteBySubmittedHomeworkAsync(command.SubmittedHomeworkId, cancellationToken);
        await unitOfWork.SubmittedHomeworkRepository.DeleteAsync(command.SubmittedHomeworkId, cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new Success();
    }
}
