using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;
using Peerly.Core.Persistence.Repositories.Files.Models;

namespace Peerly.Core.Persistence.Repositories.Files;

internal static class FileRepositoryMapper
{
    public static File ToFile(this FileDb db)
    {
        return new File
        {
            Id = new FileId(db.Id),
            StorageId = (StorageId)db.StorageId,
            Name = db.Name,
            Size = db.Size
        };
    }
}
