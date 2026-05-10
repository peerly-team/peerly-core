using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.DeleteHomeworkFile;

internal sealed class DeleteHomeworkFileHandler : ICommandHandler<DeleteHomeworkFileCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ICommandValidator<DeleteHomeworkFileCommand, Success> _validator;

    public DeleteHomeworkFileHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        ICommandValidator<DeleteHomeworkFileCommand, Success> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(DeleteHomeworkFileCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        await unitOfWork.HomeworkFileRepository.DeleteAsync(command.HomeworkId, command.FileId, cancellationToken);

        return new Success();
    }
}
