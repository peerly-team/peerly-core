using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetStudentHomework;

public sealed record GetStudentHomeworkQuery : IQuery<GetStudentHomeworkQueryResponse>
{
    public required HomeworkId HomeworkId { get; init; }
    public required StudentId StudentId { get; init; }
}
