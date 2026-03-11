using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Persistence.Repositories.Homeworks.Models;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.Homeworks;

internal sealed class HomeworkRepository : IHomeworkRepository
{
    private readonly IConnectionContext _connectionContext;

    public HomeworkRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<int> GetHomeworkCountAsync(CourseId courseId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseId = (long)courseId
        };

        const string Query =
            $"""
             select count(*)
               from {HomeworkTable.TableName}
              where {HomeworkTable.CourseId} = @{nameof(queryParams.CourseId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        return await _connectionContext.Connection.ExecuteScalarAsync<int>(command);
    }

    public async Task<IReadOnlyCollection<Homework>> ListAsync(HomeworkFilter filter, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseIds = filter.CourseIds.ToArrayBy(courseId => (long)courseId),
            HomeworkStatuses = filter.HomeworkStatuses.ToArrayBy(homeworkStatus => homeworkStatus.ToString())
        };

        const string Query =
            $"""
              select {HomeworkTable.Id},
                     {HomeworkTable.CourseId},
                     {HomeworkTable.TeacherId},
                     {HomeworkTable.Name},
                     {HomeworkTable.Description},
                     {HomeworkTable.Checklist},
                     {HomeworkTable.Deadline},
                     {HomeworkTable.ReviewDeadline}
                from {HomeworkTable.TableName}
               where (cardinality(@{nameof(queryParams.CourseIds)}) = 0
                     or {HomeworkTable.CourseId} = any(@{nameof(queryParams.CourseIds)}))
                 and (cardinality(@{nameof(queryParams.HomeworkStatuses)}) = 0
                     or {HomeworkTable.Status} = any(@{nameof(queryParams.HomeworkStatuses)}));
              """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var homeworkDbs = await _connectionContext.Connection.QueryAsync<HomeworkDb>(command);

        return homeworkDbs.ToArrayBy(homeworkDb => homeworkDb.ToHomework());
    }
}
