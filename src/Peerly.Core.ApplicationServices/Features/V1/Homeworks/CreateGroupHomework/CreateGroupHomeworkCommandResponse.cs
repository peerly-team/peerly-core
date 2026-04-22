using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework;

public sealed record CreateGroupHomeworkCommandResponse
{
    public required HomeworkId HomeworkId { get; init; }
}
