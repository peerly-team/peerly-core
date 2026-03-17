using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Storage.GenerateDownloadUrl;
using Peerly.Core.ApplicationServices.Features.V1.Storage.GenerateUploadUrl;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Storage;

[ExcludeFromCodeCoverage]
public sealed class StorageController : StorageService.StorageServiceBase
{
    private readonly IQueryHandler<GenerateUploadUrlQuery, GenerateUploadUrlQueryResponse> _generateUploadUrlHandler;
    private readonly IQueryHandler<GenerateDownloadUrlQuery, GenerateDownloadUrlQueryResponse> _generateDownloadUrlHandler;

    public StorageController(
        IQueryHandler<GenerateUploadUrlQuery, GenerateUploadUrlQueryResponse> generateUploadUrlHandler,
        IQueryHandler<GenerateDownloadUrlQuery, GenerateDownloadUrlQueryResponse> generateDownloadUrlHandler)
    {
        _generateUploadUrlHandler = generateUploadUrlHandler;
        _generateDownloadUrlHandler = generateDownloadUrlHandler;
    }

    public override async Task<V1GenerateUploadUrlResponse> V1GenerateUploadUrl(
        V1GenerateUploadUrlRequest request,
        ServerCallContext context)
    {
        var query = request.ToGenerateUploadUrlQuery();
        var queryResponse = await _generateUploadUrlHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GenerateUploadUrlResponse();
    }

    public override async Task<V1GenerateDownloadUrlResponse> V1GenerateDownloadUrl(
        V1GenerateDownloadUrlRequest request,
        ServerCallContext context)
    {
        var query = request.ToGenerateDownloadUrlQuery();
        var queryResponse = await _generateDownloadUrlHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GenerateDownloadUrlResponse();
    }
}
