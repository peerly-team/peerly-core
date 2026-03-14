using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.UpdateHomeworkStatus;

internal sealed class UpdateHomeworkStatusHandler : ICommandHandler<UpdateHomeworkStatusCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public UpdateHomeworkStatusHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(UpdateHomeworkStatusCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        // todo: добавить проверку, что препод может редактировать статусы на этой домашке (относится к курсу, для которой эта домашка)
        // todo: добавить проверку на переходы статусов (продумать статусную модель и отрисовать в miro)

        var isSuccess = await unitOfWork.HomeworkRepository.UpdateAsync(
            command.HomeworkId,
            builder => builder
                .Set(item => item.Status, command.HomeworkStatus),
            cancellationToken);

        return isSuccess ? new Success() : OtherError.NotFound();
    }
}
