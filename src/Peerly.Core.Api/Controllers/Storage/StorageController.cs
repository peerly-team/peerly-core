using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Storage.GenerateUploadUrl;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Storage;

[ExcludeFromCodeCoverage]
public sealed class StorageController : StorageService.StorageServiceBase
{
    private readonly IQueryHandler<GenerateUploadUrlQuery, GenerateUploadUrlQueryResponse> _generateUploadUrlHandler;

    public StorageController(IQueryHandler<GenerateUploadUrlQuery, GenerateUploadUrlQueryResponse> generateUploadUrlHandler)
    {
        _generateUploadUrlHandler = generateUploadUrlHandler;
    }

    public override async Task<V1GenerateUploadUrlResponse> V1GenerateUploadUrl(V1GenerateUploadUrlRequest request, ServerCallContext context)
    {
        var query = request.ToGenerateUploadUrlQuery();
        var queryResponse = await _generateUploadUrlHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GenerateUploadUrlResponse();
    }

    public override Task<V1GenerateDownloadUrlResponse> V1GenerateDownloadUrl(V1GenerateDownloadUrlRequest request, ServerCallContext context)
    {
        return base.V1GenerateDownloadUrl(request, context);
    }
}
