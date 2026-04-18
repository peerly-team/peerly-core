using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Storage.GenerateDownloadUrl;

public sealed record GenerateDownloadUrlQuery : IQuery<GenerateDownloadUrlQueryResponse>
{
    public required FileId FileId { get; init; }
    public bool IsReviewer { get; init; }
}
