using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Persistence.UnitOfWork;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.HomeworkSubmissions;

internal sealed class HomeworkSubmissionRepository : IHomeworkSubmissionRepository
{
    private readonly IConnectionContext _connectionContext;

    public HomeworkSubmissionRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<HomeworkSubmissionId> AddAsync(HomeworkSubmissionAddItem item, CancellationToken cancellationToken)
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
             insert into {HomeworkSubmissionTable.TableName} (
                         {HomeworkSubmissionTable.HomeworkId},
                         {HomeworkSubmissionTable.StudentId},
                         {HomeworkSubmissionTable.Comment},
                         {HomeworkSubmissionTable.CreationTime})
                  values (
                         @{nameof(queryParams.HomeworkId)},
                         @{nameof(queryParams.StudentId)},
                         @{nameof(queryParams.Comment)},
                         @{nameof(queryParams.CreationTime)})
               returning {HomeworkSubmissionTable.Id};
             """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var homeworkSubmissionId = await _connectionContext.Connection.QuerySingleAsync<long>(command);

        return new HomeworkSubmissionId(homeworkSubmissionId);
    }
}
