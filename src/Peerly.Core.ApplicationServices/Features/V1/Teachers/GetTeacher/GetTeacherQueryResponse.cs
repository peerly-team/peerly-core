using Peerly.Core.Models.Teachers;

namespace Peerly.Core.ApplicationServices.Features.V1.Teachers.GetTeacher;

public sealed record GetTeacherQueryResponse
{
    public required Teacher Teacher { get; init; }
}
