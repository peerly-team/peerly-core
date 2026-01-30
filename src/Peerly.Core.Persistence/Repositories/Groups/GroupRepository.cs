using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;
using Peerly.Core.Persistence.UnitOfWork;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.Groups;

internal sealed class GroupRepository : IGroupRepository
{
    private readonly IConnectionContext _connectionContext;

    public GroupRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<IReadOnlyCollection<Group>> ListAsync(IEnumerable<long> courseIds, CancellationToken cancellationToken)
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
             select {GroupTable.Id},
                    {GroupTable.CourseId},
                    {GroupTable.Name}
               from {GroupTable.TableName}
              where {GroupTable.CourseId} = any(@{nameof(queryParams.CourseIds)})
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        return [.. await _connectionContext.Connection.QueryAsync<Group>(command)];
    }

    public async Task<IReadOnlyCollection<CourseGroupStudentCount>> ListCourseGroupStudentCountAsync(
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
             select g.{GroupTable.Id} as group_id,
                    count(gs.{GroupStudentTable.Id}) as student_count
               from {GroupTable.TableName} g
               left join {GroupStudentTable.TableName} gs on gs.{GroupStudentTable.GroupId} = g.{GroupTable.Id}
              where g.{GroupTable.CourseId} = any(@{nameof(queryParams.CourseIds)})
              group by g.{GroupTable.Id};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        return [.. await _connectionContext.Connection.QueryAsync<CourseGroupStudentCount>(command)];
    }
}
