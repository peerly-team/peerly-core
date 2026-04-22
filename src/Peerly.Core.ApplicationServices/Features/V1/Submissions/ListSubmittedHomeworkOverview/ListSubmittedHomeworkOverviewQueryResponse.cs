using System.Collections.Generic;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.ListSubmittedHomeworkOverview;

public sealed record ListSubmittedHomeworkOverviewQueryResponse
{
    public required IReadOnlyCollection<SubmittedHomeworkOverview> SubmittedHomeworkOverviews { get; init; }
}
