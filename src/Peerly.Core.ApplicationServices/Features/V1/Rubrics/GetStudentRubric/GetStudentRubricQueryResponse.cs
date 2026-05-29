using System.Collections.Generic;
using Peerly.Core.Models.Rubrics;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.GetStudentRubric;

public sealed record GetStudentRubricQueryResponse
{
    public required IReadOnlyCollection<RubricCriterion> Criteria { get; init; }
}
