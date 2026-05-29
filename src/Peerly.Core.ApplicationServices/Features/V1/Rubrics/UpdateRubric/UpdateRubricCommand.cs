using System.Collections.Generic;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.Shared.Models;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.UpdateRubric;

public sealed record UpdateRubricCommand : ICommand<Success>
{
    public required RubricId RubricId { get; init; }
    public required TeacherId TeacherId { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyCollection<RubricCriterionInput> Criteria { get; init; }
}
