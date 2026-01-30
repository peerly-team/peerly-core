using System;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Abstractions.ApplicationServices;

public interface IStorage
{
    Task<Uri> GenerateUploadUrlAsync(TemporaryStorageId temporaryStorageId);
}
