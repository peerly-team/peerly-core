using System.Collections.Generic;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.Shared.GetHomework;

public sealed record HomeworkDetailResponseItem
{
    public required Homework Homework { get; init; }
    public required IReadOnlyCollection<File> Files { get; init; }
}
