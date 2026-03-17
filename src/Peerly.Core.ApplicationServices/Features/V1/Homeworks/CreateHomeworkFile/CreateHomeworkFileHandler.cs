using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomeworkFile;

internal sealed class CreateHomeworkFileHandler
    : ICommandHandler<CreateHomeworkFileCommand, CreateHomeworkFileCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IClock _clock;

    public CreateHomeworkFileHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory, IClock clock)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _clock = clock;
    }

    public async Task<CommandResponse<CreateHomeworkFileCommandResponse>> ExecuteAsync(
        CreateHomeworkFileCommand command,
        CancellationToken cancellationToken)
    {
        // todo: добавить проверку, что препод может добавлять файлы к этой домашке

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);
        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        // todo: вынести в маппер
        var fileAddItem = new FileAddItem
        {
            StorageId = command.StorageId,
            Name = command.FileName,
            Size = command.FileSize,
            CreationTime = _clock.GetCurrentTime()
        };
        var fileId = await unitOfWork.FileRepository.AddAsync(fileAddItem, cancellationToken);

        var homeworkFileAddItem = new HomeworkFileAddItem
        {
            HomeworkId = command.HomeworkId,
            FileId = fileId,
            TeacherId = command.TeacherId
        };
        _ = await unitOfWork.HomeworkFileRepository.AddAsync(homeworkFileAddItem, cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new CreateHomeworkFileCommandResponse
        {
            FileId = fileId
        };
    }
}
