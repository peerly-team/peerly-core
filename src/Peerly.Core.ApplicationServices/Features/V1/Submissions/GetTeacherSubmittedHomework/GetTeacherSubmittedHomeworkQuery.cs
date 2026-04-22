using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetTeacherSubmittedHomework;

public sealed record GetTeacherSubmittedHomeworkQuery : IQuery<GetTeacherSubmittedHomeworkQueryResponse>
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required TeacherId TeacherId { get; init; }
}
