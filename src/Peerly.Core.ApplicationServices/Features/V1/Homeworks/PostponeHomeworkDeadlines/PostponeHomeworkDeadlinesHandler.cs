using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.PostponeHomeworkDeadlines.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.PostponeHomeworkDeadlines;

internal sealed class PostponeHomeworkDeadlinesHandler : ICommandHandler<PostponeHomeworkDeadlinesCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IPostponeHomeworkDeadlinesValidator _validator;

    public PostponeHomeworkDeadlinesHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IPostponeHomeworkDeadlinesValidator validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        PostponeHomeworkDeadlinesCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        if (command.Deadline is null && command.ReviewDeadline is null)
        {
            return new Success();
        }

        var validationError = await _validator.ValidateAsync(unitOfWork, command, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        await unitOfWork.HomeworkRepository.UpdateAsync(
            command.HomeworkId,
            builder =>
            {
                if (command.Deadline is not null)
                {
                    builder.Set(item => item.Deadline, command.Deadline.Value);
                }

                if (command.ReviewDeadline is not null)
                {
                    builder.Set(item => item.ReviewDeadline, command.ReviewDeadline.Value);
                }
            },
            cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new Success();
    }
}
