using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.ListTeacherRubrics;

public sealed record ListTeacherRubricsQuery : IQuery<ListTeacherRubricsQueryResponse>
{
    public required TeacherId TeacherId { get; init; }
}
