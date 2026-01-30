using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.ApplicationServices.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Storage.GenerateUploadUrl;

internal sealed class GenerateUploadUrlHandler : IQueryHandler<GenerateUploadUrlQuery, GenerateUploadUrlQueryResponse>
{
    private readonly IFileImportService _fileImportService;

    public GenerateUploadUrlHandler(IFileImportService fileImportService)
    {
        _fileImportService = fileImportService;
    }

    public async Task<GenerateUploadUrlQueryResponse> ExecuteAsync(
        GenerateUploadUrlQuery query,
        CancellationToken cancellationToken)
    {
        var urlInfo = await _fileImportService.GenerateUploadUrl();

        return new GenerateUploadUrlQueryResponse
        {
            Url = urlInfo.Url,
            StorageId = urlInfo.StorageId
        };
    }
}
