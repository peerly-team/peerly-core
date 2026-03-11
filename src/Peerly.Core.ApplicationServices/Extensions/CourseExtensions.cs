using System.Collections.Generic;
using System.Linq;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Extensions;

internal static class CourseExtensions
{
    public static Dictionary<CourseId, int> ToHomeworkCountByCourseId(
        this IReadOnlyCollection<Homework> homeworks,
        IReadOnlyCollection<CourseId> courseIds)
    {
        var homeworkCounts = homeworks
            .GroupBy(homework => homework.CourseId)
            .ToDictionary(g => g.Key, g => g.Count());

        return courseIds.ToDictionary(
            courseId => courseId,
            courseId => homeworkCounts.GetValueOrDefault(courseId));
    }
}
