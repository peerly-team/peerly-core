using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomeworkFile;

internal sealed class DeleteSubmittedHomeworkFileHandler : ICommandHandler<DeleteSubmittedHomeworkFileCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ICommandValidator<DeleteSubmittedHomeworkFileCommand, Success> _validator;

    public DeleteSubmittedHomeworkFileHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        ICommandValidator<DeleteSubmittedHomeworkFileCommand, Success> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        DeleteSubmittedHomeworkFileCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        await unitOfWork.SubmittedHomeworkFileRepository.DeleteAsync(command.SubmittedHomeworkId, command.FileId, cancellationToken);

        return new Success();
    }
}
