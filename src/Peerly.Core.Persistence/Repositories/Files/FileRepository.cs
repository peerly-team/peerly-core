using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;
using Peerly.Core.Persistence.Repositories.Files.Models;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
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

    public async Task<IReadOnlyCollection<File>> ListByIdsAsync(IReadOnlyCollection<FileId> fileIds, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            FileIds = fileIds.ToArrayBy(fileId => (long)fileId)
        };

        const string Query =
            $"""
             select {FileTable.Id},
                    {FileTable.StorageId},
                    {FileTable.Name},
                    {FileTable.Size}
               from {FileTable.TableName}
              where {FileTable.Id} = any(@{nameof(queryParams.FileIds)});
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var results = await _connectionContext.Connection.QueryAsync<FileDb>(command);

        return results.ToArrayBy(db => db.ToFile()!);
    }

    public async Task<FileId> AddAsync(FileAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            StorageId = (Guid)item.StorageId,
            item.Name,
            item.Size,
            item.CreationTime
        };

        const string Query =
            $"""
             insert into {FileTable.TableName} (
                         {FileTable.StorageId},
                         {FileTable.Name},
                         {FileTable.Size},
                         {FileTable.CreationTime})
                  values (
                         @{nameof(queryParams.StorageId)},
                         @{nameof(queryParams.Name)},
                         @{nameof(queryParams.Size)},
                         @{nameof(queryParams.CreationTime)})
               returning {FileTable.Id};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var fileId = await _connectionContext.Connection.QuerySingleAsync<long>(command);

        return new FileId(fileId);
    }
}
