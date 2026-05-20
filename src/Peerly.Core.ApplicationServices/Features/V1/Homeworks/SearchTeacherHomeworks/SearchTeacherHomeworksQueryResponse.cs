using System.Collections.Generic;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchTeacherHomeworks;

public sealed record SearchTeacherHomeworksQueryResponse
{
    public required IReadOnlyCollection<TeacherHomeworkInfo> TeacherHomeworks { get; init; }
}
