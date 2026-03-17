using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Files;

public sealed record FileAddItem
{
    public required StorageId StorageId { get; init; }
    public required string Name { get; init; }
    public required int Size { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
