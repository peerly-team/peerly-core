using System;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Storage;

namespace Peerly.Core.FileStorage.Services;

internal sealed class FileImportService : IFileImportService
{
    private readonly IStorage _storage;

    public FileImportService(IStorage storage)
    {
        _storage = storage;
    }

    public async Task<GeneratedUploadUrlInfo> GenerateUploadUrl()
    {
        var storageId = (StorageId)Guid.NewGuid();
        var url = await _storage.GenerateUploadUrlAsync(storageId);

        return new GeneratedUploadUrlInfo
        {
            StorageId = storageId,
            Url = url
        };
    }

    public Task<Uri> GenerateDownloadUrl(StorageId storageId, string fileName)
    {
        return _storage.GenerateDownloadUrlAsync(storageId, fileName);
    }
}
