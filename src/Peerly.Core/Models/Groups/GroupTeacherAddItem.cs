using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Groups;

public sealed record GroupTeacherAddItem
{
    public required GroupId GroupId { get; init; }
    public required TeacherId TeacherId { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
