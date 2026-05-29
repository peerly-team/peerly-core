using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Persistence.Repositories.SubmittedReviewScores.Models;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.SubmittedReviewScores;

internal sealed class SubmittedReviewScoreRepository : ISubmittedReviewScoreRepository
{
    private readonly IConnectionContext _connectionContext;

    public SubmittedReviewScoreRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task BatchAddAsync(IReadOnlyCollection<SubmittedReviewScoreAddItem> items, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            SubmittedReviewIds = items.ToArrayBy(i => (long)i.SubmittedReviewId),
            RubricCriterionIds = items.ToArrayBy(i => (long)i.RubricCriterionId),
            Scores = items.ToArrayBy(i => i.Score),
            Comments = items.ToArrayBy(i => i.Comment)
        };

        const string Query = $"""
            insert into {SubmittedReviewScoreTable.TableName} (
                        {SubmittedReviewScoreTable.SubmittedReviewId},
                        {SubmittedReviewScoreTable.RubricCriterionId},
                        {SubmittedReviewScoreTable.Score},
                        {SubmittedReviewScoreTable.Comment})
            select * from unnest(
                @{nameof(queryParams.SubmittedReviewIds)},
                @{nameof(queryParams.RubricCriterionIds)},
                @{nameof(queryParams.Scores)},
                @{nameof(queryParams.Comments)});
            """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        await _connectionContext.Connection.ExecuteAsync(command);
    }

    public async Task DeleteBySubmittedReviewIdAsync(SubmittedReviewId submittedReviewId, CancellationToken cancellationToken)
    {
        var queryParams = new { SubmittedReviewId = (long)submittedReviewId };

        const string Query = $"""
            delete from {SubmittedReviewScoreTable.TableName}
                  where {SubmittedReviewScoreTable.SubmittedReviewId} = @{nameof(queryParams.SubmittedReviewId)};
            """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        await _connectionContext.Connection.ExecuteAsync(command);
    }

    public async Task<IReadOnlyCollection<SubmittedReviewScore>> ListBySubmittedReviewIdAsync(
        SubmittedReviewId submittedReviewId,
        CancellationToken cancellationToken)
    {
        var queryParams = new { SubmittedReviewId = (long)submittedReviewId };

        const string Query = $"""
            select {SubmittedReviewScoreTable.Id},
                   {SubmittedReviewScoreTable.SubmittedReviewId},
                   {SubmittedReviewScoreTable.RubricCriterionId},
                   {SubmittedReviewScoreTable.Score},
                   {SubmittedReviewScoreTable.Comment}
              from {SubmittedReviewScoreTable.TableName}
             where {SubmittedReviewScoreTable.SubmittedReviewId} = @{nameof(queryParams.SubmittedReviewId)};
            """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        var scores = await _connectionContext.Connection.QueryAsync<SubmittedReviewScoreDb>(command);
        return scores.ToArrayBy(s => s.ToSubmittedReviewScore());
    }

    public async Task<IReadOnlyCollection<SubmittedReviewScore>> ListBySubmittedReviewIdsAsync(
        IReadOnlyCollection<SubmittedReviewId> submittedReviewIds,
        CancellationToken cancellationToken)
    {
        if (submittedReviewIds.Count == 0)
            return [];

        var queryParams = new { SubmittedReviewIds = submittedReviewIds.Select(id => (long)id).ToArray() };

        const string Query = $"""
            select {SubmittedReviewScoreTable.Id},
                   {SubmittedReviewScoreTable.SubmittedReviewId},
                   {SubmittedReviewScoreTable.RubricCriterionId},
                   {SubmittedReviewScoreTable.Score},
                   {SubmittedReviewScoreTable.Comment}
              from {SubmittedReviewScoreTable.TableName}
             where {SubmittedReviewScoreTable.SubmittedReviewId} = any(@{nameof(queryParams.SubmittedReviewIds)});
            """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        var scores = await _connectionContext.Connection.QueryAsync<SubmittedReviewScoreDb>(command);
        return scores.ToArrayBy(s => s.ToSubmittedReviewScore());
    }
}
