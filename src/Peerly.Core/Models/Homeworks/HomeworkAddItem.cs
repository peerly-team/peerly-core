using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Homeworks;

public sealed record HomeworkAddItem
{
    public required CourseId CourseId { get; init; }
    public required GroupId? GroupId { get; init; }
    public required TeacherId TeacherId { get; init; }
    public required string Name { get; init; }
    public required HomeworkStatus Status { get; init; }
    public required int AmountOfReviewers { get; init; }
    public required string? Description { get; init; }
    public required string Checklist { get; init; }
    public required DateTimeOffset Deadline { get; init; }
    public required DateTimeOffset ReviewDeadline { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
