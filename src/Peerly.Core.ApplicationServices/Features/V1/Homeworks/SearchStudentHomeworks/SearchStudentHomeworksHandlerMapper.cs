using System.Collections.Generic;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchStudentHomeworks;

internal static class SearchStudentHomeworksHandlerMapper
{
    private static readonly HomeworkStatus[] s_visibleStatuses =
    [
        HomeworkStatus.Published,
        HomeworkStatus.Reviewing,
        HomeworkStatus.Confirmation,
        HomeworkStatus.Finished
    ];

    public static IReadOnlyCollection<HomeworkStatus> ResolveStatuses(this SearchStudentHomeworksQueryFilter filter)
    {
        return filter.HomeworkStatuses.Count > 0
            ? filter.HomeworkStatuses
            : s_visibleStatuses;
    }
}
