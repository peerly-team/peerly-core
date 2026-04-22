using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetSubmittedHomework;

public sealed record GetSubmittedHomeworkQuery : IQuery<GetSubmittedHomeworkQueryResponse>
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required StudentId StudentId { get; init; }
}
