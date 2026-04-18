using System.Collections.Generic;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Groups;

public sealed record GroupTeacherFilter
{
    public required IReadOnlyCollection<GroupId> GroupIds { get; init; }
    public required IReadOnlyCollection<TeacherId> TeacherIds { get; init; }

    public static GroupTeacherFilter Empty()
    {
        return new GroupTeacherFilter
        {
            GroupIds = [],
            TeacherIds = []
        };
    }
}
