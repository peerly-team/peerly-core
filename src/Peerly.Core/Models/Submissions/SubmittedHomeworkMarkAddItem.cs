using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Submissions;

public sealed record SubmittedHomeworkMarkAddItem
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required int ReviewersMark { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
