using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.UpdateDraftHomework.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.UpdateDraftHomework;

internal sealed class UpdateDraftHomeworkHandler : ICommandHandler<UpdateDraftHomeworkCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IUpdateDraftHomeworkValidator _validator;

    public UpdateDraftHomeworkHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IUpdateDraftHomeworkValidator validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        UpdateDraftHomeworkCommand command,
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
                .Set(item => item.Name, command.Name)
                .Set(item => item.AmountOfReviewers, command.AmountOfReviewers)
                .Set(item => item.Description, command.Description)
                .Set(item => item.Checklist, command.Checklist)
                .Set(item => item.Deadline, command.Deadline)
                .Set(item => item.ReviewDeadline, command.ReviewDeadline)
                .Set(item => item.DiscrepancyThreshold, command.DiscrepancyThreshold),
            cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new Success();
    }
}
