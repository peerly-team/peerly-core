using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.PublishHomework.Abstractions;
using Peerly.Core.Models.BackgroundService;
using Peerly.Core.Models.BackgroundService.HomeworkDistributions;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.PublishHomework;

internal sealed class PublishHomeworkHandlerMapper : IPublishHomeworkHandlerMapper
{
    private readonly IClock _clock;

    public PublishHomeworkHandlerMapper(IClock clock)
    {
        _clock = clock;
    }

    public HomeworkDistributionAddItem ToHomeworkDistributionAddItem(PublishHomeworkCommand command, Homework homework)
    {
        return new HomeworkDistributionAddItem
        {
            HomeworkId = command.HomeworkId,
            DistributionTime = homework.Deadline,
            CreationTime = _clock.GetCurrentTime(),
            ProcessStatus = ProcessStatus.Created,
            FailCount = 0
        };
    }
}
