using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse;

public sealed record CreateCourseCommand : ICommand<Success>
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required TeacherId TeacherId { get; init; }
}
