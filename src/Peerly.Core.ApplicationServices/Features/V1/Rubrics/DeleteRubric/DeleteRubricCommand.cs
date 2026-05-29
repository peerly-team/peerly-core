using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.DeleteRubric;

public sealed record DeleteRubricCommand : ICommand<Success>
{
    public required RubricId RubricId { get; init; }
    public required TeacherId TeacherId { get; init; }
}
