using System;

namespace Peerly.Core.Persistence.Repositories.Homeworks.Models;

internal sealed record StudentHomeworkInfoDb
{
    public required long Id { get; init; }
    public required string Name { get; init; }
    public required string Status { get; init; }
    public required int AmountOfReviewers { get; init; }
    public required string CheckList { get; init; }
    public required string? Description { get; init; }
    public required DateTimeOffset Deadline { get; init; }
    public required DateTimeOffset ReviewDeadline { get; init; }
    public required bool IsHomeworkSubmitted { get; init; }
}
