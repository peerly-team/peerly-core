using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Storage;

public sealed record GeneratedUploadUrlInfo
{
    public required Uri Url { get; init; }
    public required StorageId StorageId { get; init; }
};
