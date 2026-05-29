using System.Collections.Generic;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.Shared.Models;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.CreateRubric;

public sealed record CreateRubricCommand : ICommand<CreateRubricCommandResponse>
{
    public required TeacherId TeacherId { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyCollection<RubricCriterionInput> Criteria { get; init; }
}
