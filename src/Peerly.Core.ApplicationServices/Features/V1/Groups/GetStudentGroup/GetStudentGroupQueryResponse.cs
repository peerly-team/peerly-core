using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.GetStudentGroup;

public sealed record GetStudentGroupQueryResponse
{
    public required Group Group { get; init; }
}
