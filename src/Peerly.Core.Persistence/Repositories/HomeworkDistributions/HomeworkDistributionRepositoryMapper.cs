using Peerly.Core.Identifiers;
using Peerly.Core.Models.BackgroundService.HomeworkDistributions;
using Peerly.Core.Persistence.Repositories.HomeworkDistributions.Models;

namespace Peerly.Core.Persistence.Repositories.HomeworkDistributions;

internal static class HomeworkDistributionRepositoryMapper
{
    public static HomeworkDistributionJobItem ToHomeworkDistributionJobItem(this HomeworkDistributionJobItemDb jobItemDb)
    {
        return new HomeworkDistributionJobItem
        {
            HomeworkId = new HomeworkId(jobItemDb.HomeworkId),
            DistributionTime = jobItemDb.DistributionTime
        };
    }
}
