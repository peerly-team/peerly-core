using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.Groups;

internal sealed class GroupRepository : IGroupRepository
{
    private readonly IConnectionContext _connectionContext;

    public GroupRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<Group?> GetAsync(GroupId groupId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            GroupId = (long)groupId
        };

        const string Query =
            $"""
             select g.{GroupTable.Id},
                    g.{GroupTable.CourseId},
                    g.{GroupTable.Name},
                    count(*) as student_count
               from {GroupTable.TableName} g
               left join {GroupStudentTable.TableName} gs on gs.{GroupStudentTable.GroupId} = g.{GroupTable.Id}
              where g.{GroupTable.Id} = @{nameof(queryParams.GroupId)}
              group by g.{GroupTable.Id}, g.{GroupTable.CourseId}, g.{GroupTable.Name};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        return await _connectionContext.Connection.QuerySingleOrDefaultAsync<Group>(command);
    }

    public async Task<bool> ExistsAsync(GroupId groupId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            GroupId = (long)groupId
        };

        const string Query =
            $"""
             select exists(select
                             from {GroupTable.TableName}
                            where {GroupTable.Id} = @{nameof(queryParams.GroupId)});
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        return await _connectionContext.Connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<IReadOnlyCollection<Group>> ListAsync(GroupFilter filter, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            GroupIds = filter.GroupIds.ToArrayBy(groupId => (long)groupId),
            CourseIds = filter.CourseIds.ToArrayBy(courseId => (long)courseId)
        };

        const string Query =
            $"""
             select g.{GroupTable.Id},
                    g.{GroupTable.CourseId},
                    g.{GroupTable.Name},
                    count(*) as student_count
               from {GroupTable.TableName} g
               left join {GroupStudentTable.TableName} gs on gs.{GroupStudentTable.GroupId} = g.{GroupTable.Id}
              where (cardinality(@{nameof(queryParams.GroupIds)}) = 0
                    or g.{GroupTable.Id} = any(@{nameof(queryParams.GroupIds)}))
                and (cardinality(@{nameof(queryParams.CourseIds)}) = 0
                    or g.{GroupTable.CourseId} = any(@{nameof(queryParams.CourseIds)}))
              group by g.{GroupTable.Id}, g.{GroupTable.CourseId}, g.{GroupTable.Name};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        return [.. await _connectionContext.Connection.QueryAsync<Group>(command)];
    }

    public async Task<GroupId> AddAsync(GroupAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseId = (long)item.CourseId,
            item.Name,
            item.CreationTime
        };

        const string Query =
            $"""
             insert into {GroupTable.TableName} (
                         {GroupTable.CourseId},
                         {GroupTable.Name},
                         {GroupTable.CreationTime})
                  values (
                         @{nameof(queryParams.CourseId)},
                         @{nameof(queryParams.Name)},
                         @{nameof(queryParams.CreationTime)})
               returning {GroupTable.Id};
             """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var groupId = await _connectionContext.Connection.QuerySingleAsync<long>(command);

        return new GroupId(groupId);
    }
}
