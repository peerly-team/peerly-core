using System.Collections.Generic;
using Peerly.Core.Models.Rubrics;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.ListTeacherRubrics;

public sealed record ListTeacherRubricsQueryResponse
{
    public required IReadOnlyCollection<Rubric> Rubrics { get; init; }
}
