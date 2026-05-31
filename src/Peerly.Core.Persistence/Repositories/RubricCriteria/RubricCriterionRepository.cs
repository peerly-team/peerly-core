using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Rubrics;
using Peerly.Core.Persistence.Repositories.RubricCriteria.Models;
using Peerly.Core.Persistence.Repositories.Rubrics;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.RubricCriteria;

internal sealed class RubricCriterionRepository : IRubricCriterionRepository
{
    private readonly IConnectionContext _connectionContext;

    public RubricCriterionRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task BatchAddAsync(IReadOnlyCollection<RubricCriterionAddItem> items, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            RubricIds = items.ToArrayBy(i => (long)i.RubricId),
            Names = items.ToArrayBy(i => i.Name),
            Descriptions = items.ToArrayBy(i => i.Description),
            MaxScores = items.ToArrayBy(i => i.MaxScore),
            CommentRequireds = items.ToArrayBy(i => i.CommentRequired),
            Positions = items.ToArrayBy(i => i.Position),
            CreationTimes = items.ToArrayBy(i => i.CreationTime)
        };

        const string Query = $"""
            insert into {RubricCriterionTable.TableName} (
                        {RubricCriterionTable.RubricId},
                        {RubricCriterionTable.Name},
                        {RubricCriterionTable.Description},
                        {RubricCriterionTable.MaxScore},
                        {RubricCriterionTable.CommentRequired},
                        {RubricCriterionTable.Position},
                        {RubricCriterionTable.CreationTime})
            select * from unnest(
                @{nameof(queryParams.RubricIds)},
                @{nameof(queryParams.Names)},
                @{nameof(queryParams.Descriptions)},
                @{nameof(queryParams.MaxScores)},
                @{nameof(queryParams.CommentRequireds)},
                @{nameof(queryParams.Positions)},
                @{nameof(queryParams.CreationTimes)});
            """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        await _connectionContext.Connection.ExecuteAsync(command);
    }

    public async Task DeleteByRubricIdAsync(RubricId rubricId, CancellationToken cancellationToken)
    {
        var queryParams = new { RubricId = (long)rubricId };

        const string Query = $"""
            delete from {RubricCriterionTable.TableName}
                  where {RubricCriterionTable.RubricId} = @{nameof(queryParams.RubricId)};
            """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        await _connectionContext.Connection.ExecuteAsync(command);
    }

    public async Task<IReadOnlyCollection<RubricCriterion>> ListByRubricIdAsync(RubricId rubricId, CancellationToken cancellationToken)
    {
        var queryParams = new { RubricId = (long)rubricId };

        const string Query = $"""
            select {RubricCriterionTable.Id},
                   {RubricCriterionTable.RubricId},
                   {RubricCriterionTable.Name},
                   {RubricCriterionTable.Description},
                   {RubricCriterionTable.MaxScore},
                   {RubricCriterionTable.CommentRequired},
                   {RubricCriterionTable.Position}
              from {RubricCriterionTable.TableName}
             where {RubricCriterionTable.RubricId} = @{nameof(queryParams.RubricId)}
             order by {RubricCriterionTable.Position};
            """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        var criteria = await _connectionContext.Connection.QueryAsync<RubricCriterionDb>(command);
        return criteria.ToArrayBy(c => c.ToRubricCriterion());
    }
}
