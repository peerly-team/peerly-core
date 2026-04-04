using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Persistence.Repositories.SubmittedHomeworks.Models;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;
using SubmittedHomeworkStudent = Peerly.Core.Models.Homeworks.SubmittedHomeworkStudent;

namespace Peerly.Core.Persistence.Repositories.SubmittedHomeworks;

internal sealed class SubmittedHomeworkRepository : ISubmittedHomeworkRepository
{
    private readonly IConnectionContext _connectionContext;

    public SubmittedHomeworkRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<SubmittedHomework?> GetAsync(SubmittedHomeworkId submittedHomeworkId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            Id = (long)submittedHomeworkId
        };

        const string Query =
            $"""
             select {SubmittedHomeworkTable.Id},
                    {SubmittedHomeworkTable.HomeworkId},
                    {SubmittedHomeworkTable.StudentId}
               from {SubmittedHomeworkTable.TableName}
              where {SubmittedHomeworkTable.Id} = @{nameof(queryParams.Id)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var db = await _connectionContext.Connection.QuerySingleOrDefaultAsync<SubmittedHomeworkDb?>(command);

        return db?.ToSubmittedHomework();
    }

    public async Task<bool> ExistsAsync(SubmittedHomeworkId submittedHomeworkId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            Id = (long)submittedHomeworkId
        };

        const string Query =
            $"""
             select exists(
                 select 1
                   from {SubmittedHomeworkTable.TableName}
                  where {SubmittedHomeworkTable.Id} = @{nameof(queryParams.Id)});
             """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        return await _connectionContext.Connection.QuerySingleAsync<bool>(command);
    }

    public async Task<SubmittedHomeworkId> AddAsync(SubmittedHomeworkAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            HomeworkId = (long)item.HomeworkId,
            StudentId = (long)item.StudentId,
            item.Comment,
            item.CreationTime
        };

        const string Query =
            $"""
             insert into {SubmittedHomeworkTable.TableName} (
                         {SubmittedHomeworkTable.HomeworkId},
                         {SubmittedHomeworkTable.StudentId},
                         {SubmittedHomeworkTable.Comment},
                         {SubmittedHomeworkTable.CreationTime})
                  values (
                         @{nameof(queryParams.HomeworkId)},
                         @{nameof(queryParams.StudentId)},
                         @{nameof(queryParams.Comment)},
                         @{nameof(queryParams.CreationTime)})
               returning {SubmittedHomeworkTable.Id};
             """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var submittedHomeworkId = await _connectionContext.Connection.QuerySingleAsync<long>(command);

        return new SubmittedHomeworkId(submittedHomeworkId);
    }

    public async Task<IReadOnlyCollection<SubmittedHomeworkStudent>> ListSubmittedHomeworkStudentAsync(
        SubmittedHomeworkFilter filter,
        CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            HomeworkIds = filter.HomeworkIds.ToArrayBy(homeworkId => (long)homeworkId)
        };

        const string Query =
            $"""
             select {SubmittedHomeworkTable.Id},
                    {SubmittedHomeworkTable.StudentId}
               from {SubmittedHomeworkTable.TableName}
              where cardinality(@{nameof(queryParams.HomeworkIds)}) = 0
                 or {SubmittedHomeworkTable.HomeworkId} = any(@{nameof(queryParams.HomeworkIds)});
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var submittedHomeworkStudentDbs = await _connectionContext.Connection.QueryAsync<SubmittedHomeworkStudentDb>(command);

        return submittedHomeworkStudentDbs.ToArrayBy(dbItem => dbItem.ToSubmittedHomeworkStudent());
    }
}
