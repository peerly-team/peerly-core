using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourseFile;

public sealed record CreateCourseFileCommandResponse
{
    public required FileId FileId { get; init; }
}
