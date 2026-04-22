using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Services.Anonymization.Models;

internal sealed record AnonymizationResponse
{
    public required StorageId AnonymizedStorageId { get; init; }
    public required int Size { get; init; }
}
