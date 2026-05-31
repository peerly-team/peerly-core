using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.CreateRubric;

public sealed record CreateRubricCommandResponse
{
    public required RubricId RubricId { get; init; }
}
