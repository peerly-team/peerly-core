using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Homeworks;

public sealed record Homework
{
    public required HomeworkId HomeworkId { get; init; }
    public required CourseId CourseId { get; init; }
    public required TeacherId TeacherId { get; init; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required string? CheckList { get; init; }
    public required DateTimeOffset? Deadline { get; init; }
    public required DateTimeOffset? ReviewDeadline { get; init; }
}
