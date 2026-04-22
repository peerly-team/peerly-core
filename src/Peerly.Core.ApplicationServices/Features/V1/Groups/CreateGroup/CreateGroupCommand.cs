using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.CreateGroup;

public sealed record CreateGroupCommand : ICommand<CreateGroupCommandResponse>
{
    public required CourseId CourseId { get; init; }
    public required string Name { get; init; }
    public required TeacherId TeacherId { get; init; }
}
