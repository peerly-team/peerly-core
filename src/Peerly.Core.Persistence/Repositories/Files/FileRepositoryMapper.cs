using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;
using Peerly.Core.Persistence.Repositories.Files.Models;

namespace Peerly.Core.Persistence.Repositories.Files;

internal static class FileRepositoryMapper
{
    public static File? ToFile(this FileDb? fileDb)
    {
        if (fileDb is null)
        {
            return null;
        }

        return new File
        {
            Id = new FileId(fileDb.Id),
            StorageId = (StorageId)fileDb.StorageId,
            Name = fileDb.Name,
            Size = fileDb.Size
        };
    }
}
