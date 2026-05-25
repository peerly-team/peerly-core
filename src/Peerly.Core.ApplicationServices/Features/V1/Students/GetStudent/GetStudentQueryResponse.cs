using Peerly.Core.Models.Students;

namespace Peerly.Core.ApplicationServices.Features.V1.Students.GetStudent;

public sealed record GetStudentQueryResponse
{
    public required Student Student { get; init; }
}
