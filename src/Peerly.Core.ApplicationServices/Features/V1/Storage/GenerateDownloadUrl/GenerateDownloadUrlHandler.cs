using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;

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
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var fileId = await GetFileIdAsync(unitOfWork, query, cancellationToken);
        var file = await unitOfWork.ReadOnlyFileRepository.Get(fileId, cancellationToken)
                   ?? throw new NotFoundException();

        var url = await _fileImportService.GenerateDownloadUrl(file.StorageId, file.Name);

        return new GenerateDownloadUrlQueryResponse
        {
            Url = url
        };
    }

    private static async Task<FileId> GetFileIdAsync(
        ICommonReadOnlyUnitOfWork unitOfWork,
        GenerateDownloadUrlQuery query,
        CancellationToken cancellationToken)
    {
        var fileId = query.FileId;
        if (!query.IsReviewer)
        {
            return fileId;
        }

        var anonymizedFileId = await unitOfWork.ReadOnlySubmittedHomeworkFileRepository.GetAnonymizedFileIdAsync(fileId, cancellationToken);
        return anonymizedFileId ?? fileId;
    }
}
