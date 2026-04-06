using Peerly.Core.ApplicationServices.Features.V1.Homeworks.Shared.GetHomework;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetStudentHomework;

public sealed record GetStudentHomeworkQueryResponse
{
    public required HomeworkDetailResponseItem HomeworkDetail { get; init; }
}
