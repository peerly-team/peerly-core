using System.Collections.Generic;
using Peerly.Core.Models.Students;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.SearchGroupStudents;

public sealed record SearchGroupStudentsQueryResponse
{
    public required IReadOnlyCollection<Student> Students { get; init; }
}
