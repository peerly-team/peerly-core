using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomeworkFile.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomeworkFile;

internal sealed class DeleteSubmittedHomeworkFileHandler : ICommandHandler<DeleteSubmittedHomeworkFileCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IDeleteSubmittedHomeworkFileValidator _validator;

    public DeleteSubmittedHomeworkFileHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IDeleteSubmittedHomeworkFileValidator validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        DeleteSubmittedHomeworkFileCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var validationError = await _validator.ValidateAsync(unitOfWork, command, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        await unitOfWork.SubmittedHomeworkFileRepository.DeleteAsync(command.SubmittedHomeworkId, command.FileId, cancellationToken);

        return new Success();
    }
}
