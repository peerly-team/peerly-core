using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.GetTeacherGroup;

public sealed record GetTeacherGroupQueryResponse
{
    public required Group Group { get; init; }
}
