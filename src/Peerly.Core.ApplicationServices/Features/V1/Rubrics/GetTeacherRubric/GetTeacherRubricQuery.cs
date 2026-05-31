using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.GetTeacherRubric;

public sealed record GetTeacherRubricQuery : IQuery<GetTeacherRubricQueryResponse>
{
    public required RubricId RubricId { get; init; }
    public required TeacherId TeacherId { get; init; }
}
