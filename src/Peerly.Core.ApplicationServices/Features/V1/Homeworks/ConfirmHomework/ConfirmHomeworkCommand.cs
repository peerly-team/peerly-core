using System.Collections.Generic;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ConfirmHomework;

public sealed record ConfirmHomeworkCommand : ICommand<Success>
{
    public required HomeworkId HomeworkId { get; init; }
    public required TeacherId TeacherId { get; init; }
    public required IReadOnlyCollection<MarkCorrection> MarkCorrections { get; init; }
}
