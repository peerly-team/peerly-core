using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Groups;

public sealed record GroupStudent
{
    public required GroupId GroupId { get; init; }
    public required StudentId StudentId { get; init; }
}
