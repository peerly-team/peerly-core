using System;

namespace Peerly.Core.Persistence.Repositories.Files.Models;

internal sealed record FileDb
{
    public required long Id { get; init; }
    public required Guid StorageId { get; init; }
    public required string Name { get; init; }
    public required int Size { get; init; }
}
