using System;

namespace Peerly.Core.Models.Homeworks;

public sealed record HomeworkUpdateItem
{
    public required string Name { get; init; }
    public required HomeworkStatus Status { get; init; }
    public required int AmountOfReviewers { get; init; }
    public required string? Description { get; init; }
    public required string Checklist { get; init; }
    public required DateTimeOffset Deadline { get; init; }
    public required DateTimeOffset ReviewDeadline { get; init; }
}
