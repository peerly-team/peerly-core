using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.ListSubmittedHomeworkOverview;

public sealed record ListSubmittedHomeworkOverviewQuery : IQuery<ListSubmittedHomeworkOverviewQueryResponse>
{
    public required HomeworkId HomeworkId { get; init; }
    public required TeacherId TeacherId { get; init; }
}
