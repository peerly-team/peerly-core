using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.DistributionReviewers;

internal sealed class DistributionReviewerRepository : IDistributionReviewerRepository
{
    private readonly IConnectionContext _connectionContext;

    public DistributionReviewerRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task BatchAddAsync(IReadOnlyCollection<DistributionReviewerAddItem> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return;
        }

        var queryParams = new
        {
            SubmittedHomeworkIds = items.ToArrayBy(item => (long)item.SubmittedHomeworkId),
            StudentIds = items.ToArrayBy(item => (long)item.StudentId)
        };

        const string Query =
            $"""
             insert into {DistributionReviewerTable.TableName} (
                         {DistributionReviewerTable.SubmittedHomeworkId},
                         {DistributionReviewerTable.StudentId})
             select *
               from unnest(@{nameof(queryParams.SubmittedHomeworkIds)},
                           @{nameof(queryParams.StudentIds)})
             on conflict do nothing;
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        await _connectionContext.Connection.ExecuteAsync(command);
    }
}
