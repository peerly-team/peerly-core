using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.CreateGroup;

public sealed record CreateGroupCommandResponse
{
    public required GroupId GroupId { get; init; }
}
