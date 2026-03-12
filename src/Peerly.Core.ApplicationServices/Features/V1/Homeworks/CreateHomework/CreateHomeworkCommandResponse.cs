using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomework;

public sealed record CreateHomeworkCommandResponse
{
    public required HomeworkId HomeworkId { get; init; }
}
