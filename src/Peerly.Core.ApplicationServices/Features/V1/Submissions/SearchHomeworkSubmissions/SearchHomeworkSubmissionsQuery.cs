using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.SearchHomeworkSubmissions;

public sealed record SearchHomeworkSubmissionsQuery : IQuery<SearchHomeworkSubmissionsQueryResponse>
{
    public required HomeworkId HomeworkId { get; init; }
    public required TeacherId TeacherId { get; init; }
}
