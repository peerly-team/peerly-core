using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Services.Anonymization.Models;

internal sealed record AnonymizationResult
{
    public required StorageId AnonymizedStorageId { get; init; }
    public required int Size { get; init; }
    public required string AnonymizedFileName { get; init; }
}
