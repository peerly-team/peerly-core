using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Homeworks;
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

    public async Task<IReadOnlyCollection<CourseHomeworkCount>> ListCourseHomeworkCountAsync(
        HomeworkFilter filter,
        CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseIds = filter.CourseIds.ToArrayBy(courseId => (long)courseId),
            HomeworkStatuses = filter.HomeworkStatuses.ToArrayBy(homeworkStatus => homeworkStatus.ToString())
        };

        const string Query =
            $"""
              select c.{CourseTable.Id} as course_id,
                     count(*) as homework_count
                from {CourseTable.TableName} c
                left join {HomeworkTable.TableName} h on h.{HomeworkTable.CourseId} = c.{CourseTable.Id}
               where (cardinality(@{nameof(queryParams.CourseIds)}) = 0
                     or c.{CourseTable.Id} = any(@{nameof(queryParams.CourseIds)}))
                 and (cardinality(@{nameof(queryParams.HomeworkStatuses)}) = 0
                     or h.{HomeworkTable.Status} = any(@{nameof(queryParams.HomeworkStatuses)}))
              group by c.{HomeworkTable.Id};
              """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        return [.. await _connectionContext.Connection.QueryAsync<CourseHomeworkCount>(command)];
    }
}
