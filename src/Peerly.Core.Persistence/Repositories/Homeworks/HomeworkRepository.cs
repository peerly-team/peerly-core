using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Models.Courses;
using Peerly.Core.Persistence.UnitOfWork;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.Homeworks;

internal sealed class HomeworkRepository : IHomeworkRepository
{
    private readonly IConnectionContext _connectionContext;

    public HomeworkRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<int> GetCountAsync(long courseId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseId = courseId
        };

        const string Query =
            $"""
             select count(*)
               from {HomeworkTable.TableName}
              where {HomeworkTable.CourseId} = @{nameof(queryParams.CourseId)}
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        return await _connectionContext.Connection.QuerySingleAsync<int>(command);
    }

    public async Task<IReadOnlyCollection<CourseHomeworkCount>> ListCourseHomeworkCountsAsync(
        IEnumerable<long> courseIds,
        CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseIds = courseIds.ToArray()
        };

        if (queryParams.CourseIds.Length == 0)
        {
            return [];
        }

        const string Query =
            $"""
              select c.{CourseTable.Id} as course_id,
                     count(*) as homework_count
                from {CourseTable.TableName} c
                left join {HomeworkTable.TableName} h on h.{HomeworkTable.CourseId} = c.{CourseTable.Id}
               where c.{CourseTable.Id} = any(@{nameof(queryParams.CourseIds)})
               group by c.{CourseTable.Id};
              """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        return [.. await _connectionContext.Connection.QueryAsync<CourseHomeworkCount>(command)];
    }
}
