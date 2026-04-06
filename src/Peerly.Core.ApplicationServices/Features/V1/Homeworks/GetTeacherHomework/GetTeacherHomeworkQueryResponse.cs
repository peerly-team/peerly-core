using Peerly.Core.ApplicationServices.Features.V1.Homeworks.Shared.GetHomework;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetTeacherHomework;

public sealed record GetTeacherHomeworkQueryResponse
{
    public required HomeworkDetailResponseItem HomeworkDetail { get; init; }
}
