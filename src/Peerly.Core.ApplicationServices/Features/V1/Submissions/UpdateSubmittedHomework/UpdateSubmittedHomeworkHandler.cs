using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.UpdateSubmittedHomework.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.UpdateSubmittedHomework;

internal sealed class UpdateSubmittedHomeworkHandler : ICommandHandler<UpdateSubmittedHomeworkCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IUpdateSubmittedHomeworkValidator _validator;

    public UpdateSubmittedHomeworkHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IUpdateSubmittedHomeworkValidator validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        UpdateSubmittedHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        var validationError = await _validator.RunAsync(command, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        await unitOfWork.SubmittedHomeworkRepository.UpdateAsync(
            command.SubmittedHomeworkId,
            builder => builder.Set(item => item.Comment, command.Comment),
            cancellationToken);

        return new Success();
    }
}
