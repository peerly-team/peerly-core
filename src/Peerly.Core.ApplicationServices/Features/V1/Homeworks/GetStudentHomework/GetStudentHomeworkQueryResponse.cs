using System.Collections.Generic;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetStudentHomework;

public sealed record GetStudentHomeworkQueryResponse
{
    public required StudentHomeworkInfo StudentHomeworkInfo { get; init; }
    public required IReadOnlyCollection<File> Files { get; init; }
    public SubmittedHomeworkId? SubmittedHomeworkId { get; init; }
}
