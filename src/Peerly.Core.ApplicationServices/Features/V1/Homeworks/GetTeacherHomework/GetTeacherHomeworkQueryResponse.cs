using System.Collections.Generic;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetTeacherHomework;

public sealed record GetTeacherHomeworkQueryResponse
{
    public required Homework Homework { get; init; }
    public required int SubmittedCount { get; init; }
    public required int TotalStudentsCount { get; init; }
    public required IReadOnlyCollection<File> Files { get; init; }
}
