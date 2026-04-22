using System.Collections.Generic;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Homeworks;

public sealed record SubmittedHomeworkFilter
{
    public required IReadOnlyCollection<HomeworkId> HomeworkIds { get; init; }

    public static SubmittedHomeworkFilter Empty()
    {
        return new SubmittedHomeworkFilter
        {
            HomeworkIds = []
        };
    }
}
