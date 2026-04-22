using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.DeleteGroup;

public sealed record DeleteGroupCommand : ICommand<Success>
{
    public required GroupId GroupId { get; init; }
    public required TeacherId TeacherId { get; init; }
}
