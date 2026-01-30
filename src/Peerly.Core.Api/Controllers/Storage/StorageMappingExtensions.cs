using System.Diagnostics.CodeAnalysis;
using Peerly.Core.ApplicationServices.Features.Storage.GenerateUploadUrl;
using Proto = Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Storage;

[ExcludeFromCodeCoverage]
internal static class StorageMappingExtensions
{
    public static GenerateUploadUrlQuery ToV1GenerateUploadUrlQuery(this Proto.V1GenerateUploadUrlRequest request)
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
}
