using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Abstractions.ApplicationServices;

public interface IStorage
{
    Task<Uri> GenerateUploadUrlAsync(StorageId storageId);
    Task<Uri> GenerateDownloadUrlAsync(StorageId storageId, string originalObjectName);
    Task<Stream> GetObjectAsync(StorageId storageId, CancellationToken cancellationToken);
    Task PutObjectAsync(StorageId storageId, Stream content, CancellationToken cancellationToken);
}
