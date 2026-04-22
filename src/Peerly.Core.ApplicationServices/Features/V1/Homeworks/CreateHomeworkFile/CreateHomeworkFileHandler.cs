using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomeworkFile.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomeworkFile;

internal sealed class CreateHomeworkFileHandler : ICommandHandler<CreateHomeworkFileCommand, CreateHomeworkFileCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ICreateHomeworkFileValidator _validator;
    private readonly IClock _clock;

    public CreateHomeworkFileHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        ICreateHomeworkFileValidator validator,
        IClock clock)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
        _clock = clock;
    }

    public async Task<CommandResponse<CreateHomeworkFileCommandResponse>> ExecuteAsync(
        CreateHomeworkFileCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var validationError = await _validator.ValidateAsync(unitOfWork, command, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        var fileAddItem = command.ToFileAddItem(_clock.GetCurrentTime());
        var fileId = await unitOfWork.FileRepository.AddAsync(fileAddItem, cancellationToken);

        var homeworkFileAddItem = command.ToHomeworkFileAddItem(fileId);
        _ = await unitOfWork.HomeworkFileRepository.AddAsync(homeworkFileAddItem, cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new CreateHomeworkFileCommandResponse
        {
            FileId = fileId
        };
    }
}
