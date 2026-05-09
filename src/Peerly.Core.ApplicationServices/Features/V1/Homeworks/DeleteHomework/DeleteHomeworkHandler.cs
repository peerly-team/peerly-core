using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.DeleteHomework;

internal sealed class DeleteHomeworkHandler : ICommandHandler<DeleteHomeworkCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ICommandValidator<DeleteHomeworkCommand, Success> _validator;

    public DeleteHomeworkHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory, ICommandValidator<DeleteHomeworkCommand, Success> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(DeleteHomeworkCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);
        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        await unitOfWork.HomeworkFileRepository.DeleteByHomeworkAsync(command.HomeworkId, cancellationToken);
        await unitOfWork.HomeworkRepository.DeleteAsync(command.HomeworkId, cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new Success();
    }
}
