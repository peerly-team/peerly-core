using System.Collections.Generic;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Groups;

public sealed record GroupStudentFilter
{
    public required IReadOnlyCollection<GroupId> GroupIds { get; init; }
    public required IReadOnlyCollection<StudentId> StudentIds { get; init; }

    public static GroupStudentFilter Empty()
    {
        return new GroupStudentFilter
        {
            GroupIds = [],
            StudentIds = []
        };
    }
}
