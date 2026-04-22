using System;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.PostponeHomeworkDeadlines;

public sealed record PostponeHomeworkDeadlinesCommand : ICommand<Success>
{
    public required HomeworkId HomeworkId { get; init; }
    public required TeacherId TeacherId { get; init; }
    public DateTimeOffset? Deadline { get; init; }
    public DateTimeOffset? ReviewDeadline { get; init; }
}
