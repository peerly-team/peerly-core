using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
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

    public async Task<IReadOnlyCollection<FileId>> ListFileIdsAsync(HomeworkId homeworkId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            HomeworkId = (long)homeworkId
        };

        const string Query =
            $"""
             select {HomeworkFileTable.FileId}
               from {HomeworkFileTable.TableName}
              where {HomeworkFileTable.HomeworkId} = @{nameof(queryParams.HomeworkId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var results = await _connectionContext.Connection.QueryAsync<long>(command);

        return results.ToArrayBy(id => new FileId(id));
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
}
