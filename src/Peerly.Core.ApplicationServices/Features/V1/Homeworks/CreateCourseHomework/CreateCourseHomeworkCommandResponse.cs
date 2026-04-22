using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateCourseHomework;

public sealed record CreateCourseHomeworkCommandResponse
{
    public required HomeworkId HomeworkId { get; init; }
}
