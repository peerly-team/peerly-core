using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Groups;

public sealed record GroupTeacher
{
    public required GroupId GroupId { get; init; }
    public required TeacherId TeacherId { get; init; }
}
