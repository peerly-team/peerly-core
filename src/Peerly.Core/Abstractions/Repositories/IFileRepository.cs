using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;

namespace Peerly.Core.Abstractions.Repositories;

public interface IFileRepository : IReadOnlyFileRepository
{

}

public interface IReadOnlyFileRepository
{
    Task<File?> Get(FileId fileId, CancellationToken cancellationToken);
}
