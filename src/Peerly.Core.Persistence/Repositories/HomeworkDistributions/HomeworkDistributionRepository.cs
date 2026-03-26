using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.BackgroundService.HomeworkDistributions;
using Peerly.Core.Persistence.Common;
using Peerly.Core.Persistence.Repositories.HomeworkDistributions.Models;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.HomeworkDistributions;

internal sealed class HomeworkDistributionRepository : IHomeworkDistributionRepository
{
    private readonly IConnectionContext _connectionContext;

    public HomeworkDistributionRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<IReadOnlyCollection<HomeworkDistributionJobItem>> TakeAsync(HomeworkDistributionFilter filter, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            ProcessStatuses = filter.ProcessStatuses.ToArrayBy(static processStatus => processStatus.ToString()),
            filter.MaxFailCount,
            filter.ProcessTimeoutSeconds
        };

        const string Query =
            $"""
             with cte as (select {HomeworkDistributionTable.HomeworkId},
                                 {HomeworkDistributionTable.DistributionTime}
                           from {HomeworkDistributionTable.TableName}
                          where (cardinality(@{nameof(queryParams.ProcessStatuses)}) = 0
                                or {HomeworkDistributionTable.ProcessStatus} = any(@{nameof(queryParams.ProcessStatuses)}))
                            and (@{nameof(queryParams.ProcessTimeoutSeconds)} is null
                                or {HomeworkDistributionTable.TakenTime} < now() - (@{nameof(queryParams.ProcessTimeoutSeconds)} || ' seconds')::interval
                                or {HomeworkDistributionTable.TakenTime} is null)
                            and (@{nameof(queryParams.MaxFailCount)} is null
                                or {HomeworkDistributionTable.FailCount} < @{nameof(queryParams.MaxFailCount)})
                            for update skip locked)
                update {HomeworkDistributionTable.TableName} as hd
                   set
                       {HomeworkDistributionTable.ProcessStatus} = 'InProgress',
                       {HomeworkDistributionTable.TakenTime} = now()
                  from cte
                 where hd.{HomeworkDistributionTable.HomeworkId} = cte.{HomeworkDistributionTable.HomeworkId}
             returning cte.{HomeworkDistributionTable.HomeworkId},
                       cte.{HomeworkDistributionTable.DistributionTime};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var dbs = await _connectionContext.Connection.QueryAsync<HomeworkDistributionJobItemDb>(command);

        return dbs.ToArrayBy(db => db.ToHomeworkDistributionJobItem());
    }

    public async Task<bool> UpdateAsync(
        HomeworkId homeworkId,
        Action<IUpdateBuilder<HomeworkDistributionUpdateItem>> configureUpdate,
        CancellationToken cancellationToken)
    {
        var builder = new UpdateBuilder<HomeworkDistributionUpdateItem>();
        configureUpdate(builder);

        var configuration = builder.Build();
        var queryParams = configuration.GetQueryParams();
        queryParams.Add($"@{nameof(homeworkId)}", (long)homeworkId);

        var query =
            $"""
             update {HomeworkDistributionTable.TableName} as new
                set {HomeworkDistributionTable.ProcessStatus} = case
                    when {configuration.GetFlagParamName(item => item.ProcessStatus)}
                    then {configuration.GetParamName(item => item.ProcessStatus)}
                    else {HomeworkDistributionTable.ProcessStatus} end,
                    {HomeworkDistributionTable.FailCount} = case
                    when {configuration.GetFlagParamName(item => item.IncrementFailCount)}
                    then {HomeworkDistributionTable.FailCount} + 1
                    else {HomeworkDistributionTable.FailCount} end,
                    {HomeworkDistributionTable.ProcessTime} = case
                    when {configuration.GetFlagParamName(item => item.ProcessTime)}
                    then {configuration.GetParamName(item => item.ProcessTime)}
                    else {HomeworkDistributionTable.ProcessTime} end,
                    {HomeworkDistributionTable.Error} = case
                    when {configuration.GetFlagParamName(item => item.Error)}
                    then {configuration.GetParamName(item => item.Error)}
                    else {HomeworkDistributionTable.Error} end
               from (select {HomeworkDistributionTable.HomeworkId}
                      from {HomeworkDistributionTable.TableName}
                     where {HomeworkDistributionTable.HomeworkId} = @{nameof(homeworkId)}
                       for update) as old
              where new.{HomeworkDistributionTable.HomeworkId} = old.{HomeworkDistributionTable.HomeworkId};
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
