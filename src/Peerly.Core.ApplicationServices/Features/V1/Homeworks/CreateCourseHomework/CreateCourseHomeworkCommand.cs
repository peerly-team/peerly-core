using System;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateCourseHomework;

public sealed record CreateCourseHomeworkCommand : ICommand<CreateCourseHomeworkCommandResponse>
{
    public required CourseId CourseId { get; init; }
    public required TeacherId TeacherId { get; init; }
    public required string Name { get; init; }
    public required int AmountOfReviewers { get; init; }
    public required string? Description { get; init; }
    public required string Checklist { get; init; }
    public required DateTimeOffset Deadline { get; init; }
    public required DateTimeOffset ReviewDeadline { get; init; }
    public required int DiscrepancyThreshold { get; init; }
}
