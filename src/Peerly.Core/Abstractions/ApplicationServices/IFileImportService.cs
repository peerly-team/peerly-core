using System;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Storage;

namespace Peerly.Core.Abstractions.ApplicationServices;

public interface IFileImportService
{
    Task<GeneratedUploadUrlInfo> GenerateUploadUrl();
    Task<Uri> GenerateDownloadUrl(StorageId storageId, string fileName);
}
