using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Persistence.Repositories.Files;
using Peerly.Core.Persistence.Repositories.Files.Models;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.SubmittedHomeworkFiles;

internal sealed class SubmittedHomeworkFileRepository : ISubmittedHomeworkFileRepository
{
    private readonly IConnectionContext _connectionContext;

    public SubmittedHomeworkFileRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<bool> AddAsync(SubmittedHomeworkFileAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            HomeworkId = (long)item.SubmittedHomeworkId,
            FileId = (long)item.FileId,
            AnonymizedFileId = (long?)item.AnonymizedFileId
        };

        const string Query =
            $"""
             insert into {SubmittedHomeworkFileTable.TableName} (
                         {SubmittedHomeworkFileTable.SubmittedHomeworkId},
                         {SubmittedHomeworkFileTable.FileId},
                         {SubmittedHomeworkFileTable.AnonymizedFileId})
                  values (
                         @{nameof(queryParams.HomeworkId)},
                         @{nameof(queryParams.FileId)},
                         @{nameof(queryParams.AnonymizedFileId)});
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var affectedRows = await _connectionContext.Connection.ExecuteAsync(command);

        return affectedRows == 1;
    }

    public async Task DeleteBySubmittedHomeworkAsync(SubmittedHomeworkId submittedHomeworkId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            SubmittedHomeworkId = (long)submittedHomeworkId
        };

        const string Query =
            $"""
             delete from {SubmittedHomeworkFileTable.TableName}
                   where {SubmittedHomeworkFileTable.SubmittedHomeworkId} = @{nameof(queryParams.SubmittedHomeworkId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        await _connectionContext.Connection.ExecuteAsync(command);
    }

    public async Task DeleteAsync(SubmittedHomeworkId submittedHomeworkId, FileId fileId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            SubmittedHomeworkId = (long)submittedHomeworkId,
            FileId = (long)fileId
        };

        const string Query =
            $"""
             delete from {SubmittedHomeworkFileTable.TableName}
                   where {SubmittedHomeworkFileTable.SubmittedHomeworkId} = @{nameof(queryParams.SubmittedHomeworkId)}
                     and {SubmittedHomeworkFileTable.FileId} = @{nameof(queryParams.FileId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        await _connectionContext.Connection.ExecuteAsync(command);
    }

    public async Task<IReadOnlyCollection<File>> ListBySubmittedHomeworkAsync(
        SubmittedHomeworkId submittedHomeworkId,
        CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            SubmittedHomeworkId = (long)submittedHomeworkId
        };

        const string Query =
            $"""
             select f.{FileTable.Id},
                    f.{FileTable.StorageId},
                    f.{FileTable.Name},
                    f.{FileTable.Size}
               from {SubmittedHomeworkFileTable.TableName} shf
               join {FileTable.TableName} f on f.{FileTable.Id} = shf.{SubmittedHomeworkFileTable.FileId}
              where shf.{SubmittedHomeworkFileTable.SubmittedHomeworkId} = @{nameof(queryParams.SubmittedHomeworkId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var dbs = await _connectionContext.Connection.QueryAsync<FileDb>(command);

        return dbs.ToArrayBy(db => db.ToFile());
    }

    public async Task<FileId?> GetAnonymizedFileIdAsync(FileId fileId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            FileId = (long)fileId
        };

        const string Query =
            $"""
             select {SubmittedHomeworkFileTable.AnonymizedFileId}
               from {SubmittedHomeworkFileTable.TableName}
              where {SubmittedHomeworkFileTable.FileId} = @{nameof(queryParams.FileId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var anonymizedFileId = await _connectionContext.Connection.QuerySingleOrDefaultAsync<long?>(command);

        return anonymizedFileId is null
            ? null
            : new FileId(anonymizedFileId.Value);
    }
}
