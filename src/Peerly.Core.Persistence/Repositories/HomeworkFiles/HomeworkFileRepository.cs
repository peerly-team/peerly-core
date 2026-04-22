using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Persistence.Repositories.Files;
using Peerly.Core.Persistence.Repositories.Files.Models;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.HomeworkFiles;

internal sealed class HomeworkFileRepository : IHomeworkFileRepository
{
    private readonly IConnectionContext _connectionContext;

    public HomeworkFileRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<IReadOnlyCollection<File>> ListFilesAsync(HomeworkId homeworkId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            HomeworkId = (long)homeworkId
        };

        const string Query =
            $"""
             select f.{FileTable.Id},
                    f.{FileTable.StorageId},
                    f.{FileTable.Name},
                    f.{FileTable.Size}
               from {HomeworkFileTable.TableName} hf
               join {FileTable.TableName} f on f.{FileTable.Id} = hf.{HomeworkFileTable.FileId}
              where hf.{HomeworkFileTable.HomeworkId} = @{nameof(queryParams.HomeworkId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var dbs = await _connectionContext.Connection.QueryAsync<FileDb>(command);

        return dbs.ToArrayBy(db => db.ToFile());
    }

    public async Task<bool> AddAsync(HomeworkFileAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            HomeworkId = (long)item.HomeworkId,
            TeacherId = (long)item.TeacherId,
            FileId = (long)item.FileId
        };

        const string Query =
            $"""
             insert into {HomeworkFileTable.TableName} (
                         {HomeworkFileTable.HomeworkId},
                         {HomeworkFileTable.TeacherId},
                         {HomeworkFileTable.FileId})
                  values (
                         @{nameof(queryParams.HomeworkId)},
                         @{nameof(queryParams.TeacherId)},
                         @{nameof(queryParams.FileId)});
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var affectedRows = await _connectionContext.Connection.ExecuteAsync(command);

        return affectedRows == 1;
    }

    public async Task DeleteByHomeworkAsync(HomeworkId homeworkId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            HomeworkId = (long)homeworkId
        };

        const string Query =
            $"""
             delete from {HomeworkFileTable.TableName}
                   where {HomeworkFileTable.HomeworkId} = @{nameof(queryParams.HomeworkId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        await _connectionContext.Connection.ExecuteAsync(command);
    }

    public async Task DeleteAsync(HomeworkId homeworkId, FileId fileId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            HomeworkId = (long)homeworkId,
            FileId = (long)fileId
        };

        const string Query =
            $"""
             delete from {HomeworkFileTable.TableName}
                   where {HomeworkFileTable.HomeworkId} = @{nameof(queryParams.HomeworkId)}
                     and {HomeworkFileTable.FileId} = @{nameof(queryParams.FileId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        await _connectionContext.Connection.ExecuteAsync(command);
    }
}
