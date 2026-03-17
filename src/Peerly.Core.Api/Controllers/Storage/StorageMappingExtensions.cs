using System.Diagnostics.CodeAnalysis;
using Peerly.Core.ApplicationServices.Features.V1.Storage.GenerateDownloadUrl;
using Peerly.Core.ApplicationServices.Features.V1.Storage.GenerateUploadUrl;
using Peerly.Core.Identifiers;
using Proto = Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Storage;

[ExcludeFromCodeCoverage]
internal static class StorageMappingExtensions
{
    public static GenerateUploadUrlQuery ToGenerateUploadUrlQuery(this Proto.V1GenerateUploadUrlRequest request)
    {
        return new GenerateUploadUrlQuery();
    }

    public static Proto.V1GenerateUploadUrlResponse ToV1GenerateUploadUrlResponse(
        this GenerateUploadUrlQueryResponse queryResponse)
    {
        return new Proto.V1GenerateUploadUrlResponse
        {
            StorageId = queryResponse.StorageId.ToString(),
            Url = queryResponse.Url.ToString()
        };
    }

    public static GenerateDownloadUrlQuery ToGenerateDownloadUrlQuery(this Proto.V1GenerateDownloadUrlRequest request)
    {
        return new GenerateDownloadUrlQuery
        {
            FileId = new FileId(request.FileId)
        };
    }

    public static Proto.V1GenerateDownloadUrlResponse ToV1GenerateDownloadUrlResponse(this GenerateDownloadUrlQueryResponse queryResponse)
    {
        return new Proto.V1GenerateDownloadUrlResponse
        {
            Url = queryResponse.Url.ToString()
        };
    }
}
