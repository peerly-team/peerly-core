using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Files;

public sealed record File
{
    public required FileId Id { get; init; }
    public required StorageId StorageId { get; init; }
    public required string Name { get; init; }
    public required int Size { get; init; }
}
