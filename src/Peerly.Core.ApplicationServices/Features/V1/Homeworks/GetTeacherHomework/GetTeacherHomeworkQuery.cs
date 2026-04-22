using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetTeacherHomework;

public sealed record GetTeacherHomeworkQuery : IQuery<GetTeacherHomeworkQueryResponse>
{
    public required HomeworkId HomeworkId { get; init; }
    public required TeacherId TeacherId { get; init; }
}
