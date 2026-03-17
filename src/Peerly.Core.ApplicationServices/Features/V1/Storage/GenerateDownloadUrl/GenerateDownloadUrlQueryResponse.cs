using System;

namespace Peerly.Core.ApplicationServices.Features.V1.Storage.GenerateDownloadUrl;

public sealed record GenerateDownloadUrlQueryResponse
{
    public required Uri Url { get; init; }
}
