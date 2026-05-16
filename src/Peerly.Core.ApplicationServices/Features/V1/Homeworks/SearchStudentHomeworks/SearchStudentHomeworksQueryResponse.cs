using System.Collections.Generic;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchStudentHomeworks;

public sealed record SearchStudentHomeworksQueryResponse
{
    public required IReadOnlyCollection<StudentHomeworkInfo> StudentHomeworks { get; init; }
}
