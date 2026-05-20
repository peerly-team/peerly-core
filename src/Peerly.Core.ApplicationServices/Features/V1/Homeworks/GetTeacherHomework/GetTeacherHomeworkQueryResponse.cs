using System.Collections.Generic;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetTeacherHomework;

public sealed record GetTeacherHomeworkQueryResponse
{
    public required TeacherHomeworkInfo TeacherHomeworkInfo { get; init; }
    public required IReadOnlyCollection<File> Files { get; init; }
    public int? SubmittedHomeworkCount { get; init; }
}
