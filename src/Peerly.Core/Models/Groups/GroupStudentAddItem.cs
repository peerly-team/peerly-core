using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Groups;

public sealed record GroupStudentAddItem
{
    public required GroupId GroupId { get; init; }
    public required StudentId StudentId { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
