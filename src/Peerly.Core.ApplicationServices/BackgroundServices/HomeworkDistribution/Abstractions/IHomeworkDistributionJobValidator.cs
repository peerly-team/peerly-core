using System.Collections.Generic;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.BackgroundServices.HomeworkDistribution.Abstractions;

internal interface IHomeworkDistributionJobValidator
{
    void Validate(IReadOnlyCollection<SubmittedHomeworkStudent> submittedHomeworks, int amountOfReviewers);
}
