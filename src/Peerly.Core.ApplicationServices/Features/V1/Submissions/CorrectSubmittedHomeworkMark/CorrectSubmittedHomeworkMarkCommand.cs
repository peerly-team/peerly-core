using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CorrectSubmittedHomeworkMark;

public sealed record CorrectSubmittedHomeworkMarkCommand : ICommand<Success>
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required TeacherId TeacherId { get; init; }
    public required int TeacherMark { get; init; }
}
