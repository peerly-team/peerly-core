using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Files;

namespace Peerly.Core.ApplicationServices.Features.V1.Storage.GenerateDownloadUrl;

internal sealed class GenerateDownloadUrlHandler : IQueryHandler<GenerateDownloadUrlQuery, GenerateDownloadUrlQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IFileImportService _fileImportService;

    public GenerateDownloadUrlHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory, IFileImportService fileImportService)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _fileImportService = fileImportService;
    }

    public async Task<GenerateDownloadUrlQueryResponse> ExecuteAsync(GenerateDownloadUrlQuery query, CancellationToken cancellationToken)
    {
        var file = await GetFileAsync(query, cancellationToken) ?? throw new NotFoundException();
        var url = await _fileImportService.GenerateDownloadUrl(file.StorageId, file.Name);

        return new GenerateDownloadUrlQueryResponse
        {
            Url = url
        };
    }

    private async Task<File?> GetFileAsync(GenerateDownloadUrlQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);
        return await unitOfWork.ReadOnlyFileRepository.Get(query.FileId, cancellationToken);
    }
}
