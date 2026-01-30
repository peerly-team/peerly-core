using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Models.Courses;
using Peerly.Core.Pagination;
using Peerly.Core.Persistence.Repositories.Courses.Models;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.Courses;

internal sealed class CourseRepository : ICourseRepository
{
    private readonly IConnectionContext _connectionContext;

    public CourseRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<IReadOnlyCollection<Course>> ListAsync(CourseFilter filter, PaginationInfo paginationInfo, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseStatuses = filter.CourseStatuses.ToArrayBy(courseStatus => courseStatus.ToString()),
            Limit = paginationInfo.PageSize,
            paginationInfo.Offset,
        };

        const string Query =
            $"""
              select {CourseTable.Id},
                     {CourseTable.Name},
                     {CourseTable.Description},
                     {CourseTable.Status}
                from {CourseTable.TableName}
               where (CARDINALITY(@{nameof(queryParams.CourseStatuses)}) = 0
                     or {CourseTable.Status} = any(@{nameof(queryParams.CourseStatuses)}))
               limit @{nameof(queryParams.Limit)}
              offset @{nameof(queryParams.Offset)};
              """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var courseDbs = await _connectionContext.Connection.QueryAsync<CourseDb>(command);

        return courseDbs.ToCourses();
    }
}
