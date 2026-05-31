using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Rubrics;
using Peerly.Core.Persistence.Repositories.Rubrics.Models;
using Peerly.Core.Persistence.Common;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.Rubrics;

internal sealed class RubricRepository : IRubricRepository
{
    private readonly IConnectionContext _connectionContext;

    public RubricRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<RubricId> AddAsync(RubricAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            TeacherId = (long)item.TeacherId,
            item.Name,
            item.CreationTime
        };

        const string Query = $"""
            insert into {RubricTable.TableName} (
                        {RubricTable.TeacherId},
                        {RubricTable.Name},
                        {RubricTable.CreationTime})
                 values (@{nameof(queryParams.TeacherId)},
                         @{nameof(queryParams.Name)},
                         @{nameof(queryParams.CreationTime)})
              returning {RubricTable.Id};
            """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        var rubricId = await _connectionContext.Connection.QuerySingleAsync<long>(command);
        return new RubricId(rubricId);
    }

    public async Task DeleteAsync(RubricId rubricId, CancellationToken cancellationToken)
    {
        var queryParams = new { RubricId = (long)rubricId };

        const string Query =
            $"""
            delete from {RubricTable.TableName}
                  where {RubricTable.Id} = @{nameof(queryParams.RubricId)};
            """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        await _connectionContext.Connection.ExecuteAsync(command);
    }

    public async Task<Rubric?> GetAsync(RubricId rubricId, CancellationToken cancellationToken)
    {
        var queryParams = new { RubricId = (long)rubricId };

        const string Query = $"""
            select {RubricTable.Id},
                   {RubricTable.TeacherId},
                   {RubricTable.Name}
              from {RubricTable.TableName}
             where {RubricTable.Id} = @{nameof(queryParams.RubricId)};
            """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        var rubricDb = await _connectionContext.Connection.QuerySingleOrDefaultAsync<RubricDb>(command);
        return rubricDb?.ToRubric();
    }

    public async Task<IReadOnlyCollection<Rubric>> ListByTeacherAsync(TeacherId teacherId, CancellationToken cancellationToken)
    {
        var queryParams = new { TeacherId = (long)teacherId };

        const string Query = $"""
            select {RubricTable.Id},
                   {RubricTable.TeacherId},
                   {RubricTable.Name}
              from {RubricTable.TableName}
             where {RubricTable.TeacherId} = @{nameof(queryParams.TeacherId)}
             order by {RubricTable.CreationTime} desc;
            """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        var rubrics = await _connectionContext.Connection.QueryAsync<RubricDb>(command);
        return rubrics.ToArrayBy(r => r.ToRubric());
    }

    public async Task<bool> UpdateAsync(
        RubricId rubricId,
        Action<IUpdateBuilder<RubricUpdateItem>> configureUpdate,
        CancellationToken cancellationToken)
    {
        var builder = new UpdateBuilder<RubricUpdateItem>();
        configureUpdate(builder);

        var configuration = builder.Build();
        var queryParams = configuration.GetQueryParams();
        queryParams.Add($"@{nameof(rubricId)}", (long)rubricId);

        var query =
            $"""
             update {RubricTable.TableName} as new
                set {RubricTable.UpdateTime} = now(),
                    {RubricTable.Name} = case
                    when {configuration.GetFlagParamName(item => item.Name)}
                    then {configuration.GetParamName(item => item.Name)}
                    else {RubricTable.Name}
                    end
               from (select {RubricTable.Id}
                       from {RubricTable.TableName}
                      where {RubricTable.Id} = @{nameof(rubricId)}
                        for update) as old
              where new.{RubricTable.Id} = old.{RubricTable.Id};
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
