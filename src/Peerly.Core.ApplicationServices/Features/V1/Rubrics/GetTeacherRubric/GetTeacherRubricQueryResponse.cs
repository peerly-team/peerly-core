using System.Collections.Generic;
using Peerly.Core.Models.Rubrics;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.GetTeacherRubric;

public sealed record GetTeacherRubricQueryResponse
{
    public required Rubric Rubric { get; init; }
    public required IReadOnlyCollection<RubricCriterion> Criteria { get; init; }
}
