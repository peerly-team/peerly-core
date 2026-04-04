using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Persistence.Common;
using Peerly.Core.Persistence.Repositories.SubmittedHomeworkMarks.Models;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.SubmittedHomeworkMarks;

internal sealed class SubmittedHomeworkMarkRepository : ISubmittedHomeworkMarkRepository
{
    private readonly IConnectionContext _connectionContext;

    public SubmittedHomeworkMarkRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task BatchAddAsync(IReadOnlyCollection<SubmittedHomeworkMarkAddItem> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return;
        }

        var queryParams = new
        {
            SubmittedHomeworkIds = items.ToArrayBy(item => (long)item.SubmittedHomeworkId),
            ReviewersMarks = items.ToArrayBy(item => item.ReviewersMark),
            CreationTimes = items.ToArrayBy(item => item.CreationTime)
        };

        const string Query =
            $"""
             insert into {SubmittedHomeworkMarkTable.TableName} (
                         {SubmittedHomeworkMarkTable.SubmittedHomeworkId},
                         {SubmittedHomeworkMarkTable.ReviewersMark},
                         {SubmittedHomeworkMarkTable.CreationTime})
             select *
               from unnest(@{nameof(queryParams.SubmittedHomeworkIds)},
                           @{nameof(queryParams.ReviewersMarks)},
                           @{nameof(queryParams.CreationTimes)})
             on conflict do nothing;
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        await _connectionContext.Connection.ExecuteAsync(command);
    }

    public async Task BatchUpdateAsync(IReadOnlyCollection<SubmittedHomeworkMarkBatchUpdateItem> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return;
        }

        var queryParams = new
        {
            SubmittedHomeworkIds = items.ToArrayBy(item => (long)item.SubmittedHomeworkId),
            TeacherMarks = items.ToArrayBy(item => item.TeacherMark),
            TeacherIds = items.ToArrayBy(item => (long)item.TeacherId),
            UpdateTimes = items.ToArrayBy(item => item.UpdateTime)
        };

        const string Query =
            $"""
             update {SubmittedHomeworkMarkTable.TableName} as shm
                set {SubmittedHomeworkMarkTable.TeacherMark} = input.{nameof(SubmittedHomeworkMarkTable.TeacherMark)},
                    {SubmittedHomeworkMarkTable.TeacherId} = input.{nameof(SubmittedHomeworkMarkTable.TeacherId)},
                    {SubmittedHomeworkMarkTable.UpdateTime} = input.{nameof(SubmittedHomeworkMarkTable.UpdateTime)}
               from unnest(@{nameof(queryParams.SubmittedHomeworkIds)},
                           @{nameof(queryParams.TeacherMarks)},
                           @{nameof(queryParams.TeacherIds)},
                           @{nameof(queryParams.UpdateTimes)})
                    as input (
                             {nameof(SubmittedHomeworkMarkTable.SubmittedHomeworkId)},
                             {nameof(SubmittedHomeworkMarkTable.TeacherMark)},
                             {nameof(SubmittedHomeworkMarkTable.TeacherId)},
                             {nameof(SubmittedHomeworkMarkTable.UpdateTime)})
              where shm.{SubmittedHomeworkMarkTable.SubmittedHomeworkId} = input.{nameof(SubmittedHomeworkMarkTable.SubmittedHomeworkId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        await _connectionContext.Connection.ExecuteAsync(command);
    }

    public async Task<bool> UpdateAsync(
        SubmittedHomeworkId submittedHomeworkId,
        Action<IUpdateBuilder<SubmittedHomeworkMarkUpdateItem>> configureUpdate,
        CancellationToken cancellationToken)
    {
        var builder = new UpdateBuilder<SubmittedHomeworkMarkUpdateItem>();
        configureUpdate(builder);

        var configuration = builder.Build();
        var queryParams = configuration.GetQueryParams();
        queryParams.Add($"@{nameof(submittedHomeworkId)}", (long)submittedHomeworkId);

        var query =
            $"""
             update {SubmittedHomeworkMarkTable.TableName} as new
                set {SubmittedHomeworkMarkTable.TeacherMark} = case
                    when {configuration.GetFlagParamName(item => item.TeacherMark)}
                    then {configuration.GetParamName(item => item.TeacherMark)}
                    else {SubmittedHomeworkMarkTable.TeacherMark}
                    end,
                    {SubmittedHomeworkMarkTable.TeacherId} = case
                    when {configuration.GetFlagParamName(item => item.TeacherId)}
                    then {configuration.GetParamName(item => item.TeacherId)}
                    else {SubmittedHomeworkMarkTable.TeacherId}
                    end,
                    {SubmittedHomeworkMarkTable.UpdateTime} = case
                    when {configuration.GetFlagParamName(item => item.UpdateTime)}
                    then {configuration.GetParamName(item => item.UpdateTime)}
                    else {SubmittedHomeworkMarkTable.UpdateTime}
                    end
              from (select {SubmittedHomeworkMarkTable.SubmittedHomeworkId}
                      from {SubmittedHomeworkMarkTable.TableName}
                     where {SubmittedHomeworkMarkTable.SubmittedHomeworkId} = @{nameof(submittedHomeworkId)}
                       for update) as old
             WHERE new.{SubmittedHomeworkMarkTable.SubmittedHomeworkId} = old.{SubmittedHomeworkMarkTable.SubmittedHomeworkId};
             """;

        var command = new CommandDefinition(
            query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var affectedRows = await _connectionContext.Connection.ExecuteAsync(command);

        return affectedRows == 1;
    }

    public async Task<IReadOnlyCollection<SubmittedHomeworkMark>> ListAsync(
        HomeworkId homeworkId,
        CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            HomeworkId = (long)homeworkId
        };

        const string Query =
            $"""
             select shm.{SubmittedHomeworkMarkTable.SubmittedHomeworkId},
                    shm.{SubmittedHomeworkMarkTable.ReviewersMark},
                    shm.{SubmittedHomeworkMarkTable.TeacherMark},
                    shm.{SubmittedHomeworkMarkTable.TeacherId}
               from {SubmittedHomeworkMarkTable.TableName} shm
               join {SubmittedHomeworkTable.TableName} sh
                 on sh.{SubmittedHomeworkTable.Id} = shm.{SubmittedHomeworkMarkTable.SubmittedHomeworkId}
              where sh.{SubmittedHomeworkTable.HomeworkId} = @{nameof(queryParams.HomeworkId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var results = await _connectionContext.Connection.QueryAsync<SubmittedHomeworkMarkDb>(command);

        return results.ToArrayBy(db => db.ToSubmittedHomeworkMark());
    }
}
