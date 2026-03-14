using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;
using Peerly.Core.Persistence.Repositories.Files.Models;
using Peerly.Core.Persistence.UnitOfWork;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.Files;

internal sealed class FileRepository : IFileRepository
{
    private readonly IConnectionContext _connectionContext;

    public FileRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<File?> Get(FileId fileId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            FileId = (long)fileId
        };

        const string Query =
            $"""
             select {FileTable.Id},
                    {FileTable.StorageId},
                    {FileTable.Name},
                    {FileTable.Size}
               from {FileTable.TableName}
              where {FileTable.Id} = @{nameof(queryParams.FileId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var fileDb = await _connectionContext.Connection.QuerySingleOrDefaultAsync<FileDb>(command);

        return fileDb.ToFile();
    }
}
