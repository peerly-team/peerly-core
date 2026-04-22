using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.UpdateGroup;

public sealed record UpdateGroupCommand : ICommand<Success>
{
    public required GroupId GroupId { get; init; }
    public required TeacherId TeacherId { get; init; }
    public required string Name { get; init; }
}
