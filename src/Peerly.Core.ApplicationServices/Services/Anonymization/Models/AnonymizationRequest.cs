using System.Collections.Generic;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Students;

namespace Peerly.Core.ApplicationServices.Services.Anonymization.Models;

internal sealed record AnonymizationRequest
{
    public required StorageId OriginalStorageId { get; init; }
    public required string FileName { get; init; }
    public required IReadOnlyCollection<Student> Students { get; init; }
}
