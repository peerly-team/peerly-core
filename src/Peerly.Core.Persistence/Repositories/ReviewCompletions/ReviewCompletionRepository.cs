using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.BackgroundService.ReviewCompletions;
using Peerly.Core.Persistence.Common;
using Peerly.Core.Persistence.Repositories.ReviewCompletions.Models;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.ReviewCompletions;

internal sealed class ReviewCompletionRepository : IReviewCompletionRepository
{
    private readonly IConnectionContext _connectionContext;

    public ReviewCompletionRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task AddAsync(ReviewCompletionAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            HomeworkId = (long)item.HomeworkId,
            item.CompletionTime,
            item.CreationTime,
            ProcessStatus = item.ProcessStatus.ToString(),
            item.FailCount
        };

        const string Query =
            $"""
             insert into {ReviewCompletionTable.TableName}
                 ({ReviewCompletionTable.HomeworkId},
                  {ReviewCompletionTable.CompletionTime},
                  {ReviewCompletionTable.CreationTime},
                  {ReviewCompletionTable.ProcessStatus},
                  {ReviewCompletionTable.FailCount})
             values
                 (@{nameof(queryParams.HomeworkId)},
                  @{nameof(queryParams.CompletionTime)},
                  @{nameof(queryParams.CreationTime)},
                  @{nameof(queryParams.ProcessStatus)},
                  @{nameof(queryParams.FailCount)});
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        await _connectionContext.Connection.ExecuteAsync(command);
    }

    public async Task<IReadOnlyCollection<ReviewCompletionJobItem>> TakeAsync(
        ReviewCompletionFilter filter,
        CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            ProcessStatuses = filter.ProcessStatuses.ToArrayBy(static processStatus => processStatus.ToString()),
            filter.MaxFailCount,
            filter.ProcessTimeoutSeconds,
            filter.Limit
        };

        const string Query =
            $"""
             with cte as (select {ReviewCompletionTable.HomeworkId},
                                 {ReviewCompletionTable.CompletionTime}
                            from {ReviewCompletionTable.TableName}
                           where (cardinality(@{nameof(queryParams.ProcessStatuses)}) = 0
                                 or {ReviewCompletionTable.ProcessStatus} = any(@{nameof(queryParams.ProcessStatuses)}))
                             and (@{nameof(queryParams.ProcessTimeoutSeconds)} is null
                                 or {ReviewCompletionTable.TakenTime} < now() - (@{nameof(queryParams.ProcessTimeoutSeconds)} || ' seconds')::interval
                                 or {ReviewCompletionTable.TakenTime} is null)
                             and (@{nameof(queryParams.MaxFailCount)} is null
                                 or {ReviewCompletionTable.FailCount} < @{nameof(queryParams.MaxFailCount)})
                             for update skip locked
                             limit @{nameof(queryParams.Limit)})
                update {ReviewCompletionTable.TableName} as rc
                   set
                       {ReviewCompletionTable.ProcessStatus} = 'InProgress',
                       {ReviewCompletionTable.TakenTime} = now()
                  from cte
                 where rc.{ReviewCompletionTable.HomeworkId} = cte.{ReviewCompletionTable.HomeworkId}
             returning cte.{ReviewCompletionTable.HomeworkId},
                       cte.{ReviewCompletionTable.CompletionTime};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var dbs = await _connectionContext.Connection.QueryAsync<ReviewCompletionJobItemDb>(command);

        return dbs.ToArrayBy(db => db.ToReviewCompletionJobItem());
    }

    public async Task<bool> UpdateAsync(
        HomeworkId homeworkId,
        Action<IUpdateBuilder<ReviewCompletionUpdateItem>> configureUpdate,
        CancellationToken cancellationToken)
    {
        var builder = new UpdateBuilder<ReviewCompletionUpdateItem>();
        configureUpdate(builder);

        var configuration = builder.Build();
        var queryParams = configuration.GetQueryParams();
        queryParams.Add($"@{nameof(homeworkId)}", (long)homeworkId);

        var query =
            $"""
             update {ReviewCompletionTable.TableName} as new
                set {ReviewCompletionTable.ProcessStatus} = case
                    when {configuration.GetFlagParamName(item => item.ProcessStatus)}
                    then {configuration.GetParamName(item => item.ProcessStatus)}
                    else {ReviewCompletionTable.ProcessStatus} end,
                    {ReviewCompletionTable.FailCount} = case
                    when {configuration.GetFlagParamName(item => item.IncrementFailCount)}
                    then {ReviewCompletionTable.FailCount} + 1
                    else {ReviewCompletionTable.FailCount} end,
                    {ReviewCompletionTable.ProcessTime} = case
                    when {configuration.GetFlagParamName(item => item.ProcessTime)}
                    then {configuration.GetParamName(item => item.ProcessTime)}
                    else {ReviewCompletionTable.ProcessTime} end,
                    {ReviewCompletionTable.Error} = case
                    when {configuration.GetFlagParamName(item => item.Error)}
                    then {configuration.GetParamName(item => item.Error)}
                    else {ReviewCompletionTable.Error} end,
                    {ReviewCompletionTable.CompletionTime} = case
                    when {configuration.GetFlagParamName(item => item.CompletionTime)}
                    then {configuration.GetParamName(item => item.CompletionTime)}
                    else {ReviewCompletionTable.CompletionTime} end,
                    {ReviewCompletionTable.TakenTime} = case
                    when {configuration.GetFlagParamName(item => item.TakenTime)}
                    then {configuration.GetParamName(item => item.TakenTime)}
                    else {ReviewCompletionTable.TakenTime} end
               from (select {ReviewCompletionTable.HomeworkId}
                      from {ReviewCompletionTable.TableName}
                     where {ReviewCompletionTable.HomeworkId} = @{nameof(homeworkId)}
                       for update) as old
              where new.{ReviewCompletionTable.HomeworkId} = old.{ReviewCompletionTable.HomeworkId};
             """;

        var command = new CommandDefinition(
            query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var affectedRows = await _connectionContext.Connection.ExecuteAsync(command);

        return affectedRows == 1;
    }
}
