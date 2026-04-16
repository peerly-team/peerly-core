using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.ConfirmHomework.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ConfirmHomework;

internal sealed class ConfirmHomeworkHandler : ICommandHandler<ConfirmHomeworkCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IConfirmHomeworkValidator _validator;

    public ConfirmHomeworkHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IConfirmHomeworkValidator validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        ConfirmHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var validationError = await _validator.ValidateAsync(unitOfWork, command, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        await unitOfWork.HomeworkRepository.UpdateAsync(
            command.HomeworkId,
            builder => builder
                .Set(item => item.Status, HomeworkStatus.Finished),
            cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new Success();
    }
}
