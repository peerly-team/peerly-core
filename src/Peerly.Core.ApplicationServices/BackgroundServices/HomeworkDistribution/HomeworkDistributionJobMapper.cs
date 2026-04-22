using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.BackgroundServices.HomeworkDistribution;

internal static class HomeworkDistributionJobMapper
{
    public static SubmittedHomeworkFilter ToSubmittedHomeworkFilter(this Homework homework)
    {
        return new SubmittedHomeworkFilter
        {
            HomeworkIds = [homework.Id]
        };
    }
}
