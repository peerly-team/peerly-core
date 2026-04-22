using Peerly.Core.Models.BackgroundService.HomeworkDistributions;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.PublishHomework.Abstractions;

internal interface IPublishHomeworkHandlerMapper
{
    HomeworkDistributionAddItem ToHomeworkDistributionAddItem(PublishHomeworkCommand command, Homework homework);
}
