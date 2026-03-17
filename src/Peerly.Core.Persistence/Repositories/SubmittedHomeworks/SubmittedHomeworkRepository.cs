using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Persistence.UnitOfWork;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.SubmittedHomeworks;

internal sealed class SubmittedHomeworkRepository : ISubmittedHomeworkRepository
{
    private readonly IConnectionContext _connectionContext;

    public SubmittedHomeworkRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
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
}
