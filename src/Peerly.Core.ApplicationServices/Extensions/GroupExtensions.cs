using System.Collections.Generic;
using System.Linq;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Extensions;

internal static class GroupExtensions
{
    public static Dictionary<CourseId, int> ToStudentCountByCourseId(
        this IReadOnlyCollection<Group> groups,
        IReadOnlyCollection<CourseId> courseIds)
    {
        var studentCounts = groups
            .GroupBy(x => x.CourseId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => x.StudentCount));

        return courseIds.ToDictionary(
            courseId => courseId,
            courseId => studentCounts.GetValueOrDefault(courseId));
    }
}
