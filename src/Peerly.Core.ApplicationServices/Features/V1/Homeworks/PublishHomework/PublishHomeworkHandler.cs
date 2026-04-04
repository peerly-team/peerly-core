using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.PublishHomework.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.PublishHomework;

internal sealed class PublishHomeworkHandler : ICommandHandler<PublishHomeworkCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IPublishHomeworkHandlerMapper _mapper;

    public PublishHomeworkHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IPublishHomeworkHandlerMapper mapper)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _mapper = mapper;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        PublishHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var homework = await unitOfWork.HomeworkRepository.GetAsync(command.HomeworkId, cancellationToken);
        if (homework is null)
        {
            return OtherError.NotFound();
        }

        if (homework.Status is not HomeworkStatus.Draft)
        {
            return OtherError.Conflict();
        }

        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        await unitOfWork.HomeworkRepository.UpdateAsync(
            command.HomeworkId,
            builder => builder
                .Set(item => item.Status, HomeworkStatus.Published),
            cancellationToken);

        var distributionAddItem = _mapper.ToHomeworkDistributionAddItem(command, homework);
        await unitOfWork.HomeworkDistributionRepository.AddAsync(distributionAddItem, cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new Success();
    }
}
