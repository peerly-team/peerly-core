using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomework;

public sealed record CreateSubmittedHomeworkCommand : ICommand<CreateSubmittedHomeworkCommandResponse>
{
    public required HomeworkId HomeworkId { get; init; }
    public required StudentId StudentId { get; init; }
    public required string Comment { get; init; }
}
