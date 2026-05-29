using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.GetStudentRubric;

public sealed record GetStudentRubricQuery : IQuery<GetStudentRubricQueryResponse>
{
    public required RubricId RubricId { get; init; }
    public required StudentId StudentId { get; init; }
}
