using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.UpdateSubmittedHomework;

public sealed record UpdateSubmittedHomeworkCommand : ICommand<Success>
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required StudentId StudentId { get; init; }
    public required string Comment { get; init; }
}
