using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Storage.GenerateUploadUrl;

public sealed record GenerateUploadUrlQueryResponse
{
    public required Uri Url { get; init; }
    public required TemporaryStorageId StorageId { get; init; }
}
