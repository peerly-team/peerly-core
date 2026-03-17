using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomeworkFile;

public sealed record CreateHomeworkFileCommandResponse
{
    public required FileId FileId { get; init; }
}
