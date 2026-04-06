using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;
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

    public async Task<IReadOnlyCollection<SubmittedHomeworkFileItem>> ListAsync(SubmittedHomeworkId submittedHomeworkId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            SubmittedHomeworkId = (long)submittedHomeworkId
        };

        const string Query =
            $"""
             select {SubmittedHomeworkFileTable.FileId},
                    {SubmittedHomeworkFileTable.AnonymizedFileId}
               from {SubmittedHomeworkFileTable.TableName}
              where {SubmittedHomeworkFileTable.SubmittedHomeworkId} = @{nameof(queryParams.SubmittedHomeworkId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var results = await _connectionContext.Connection.QueryAsync<SubmittedHomeworkFileDb>(command);

        return results.ToArrayBy(db => new SubmittedHomeworkFileItem
        {
            FileId = new FileId(db.FileId),
            AnonymizedFileId = db.AnonymizedFileId.HasValue ? new FileId(db.AnonymizedFileId.Value) : null
        });
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
