using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.PublishHomework;

public sealed record PublishHomeworkCommand : ICommand<Success>
{
    public required HomeworkId HomeworkId { get; init; }
    public required TeacherId TeacherId { get; init; }
}
