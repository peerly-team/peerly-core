using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;
using Peerly.Core.Persistence.Repositories.GroupTeachers.Models;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.GroupTeachers;

internal sealed class GroupTeacherRepository : IGroupTeacherRepository
{
    private readonly IConnectionContext _connectionContext;

    public GroupTeacherRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task AddAsync(GroupTeacherAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            GroupId = (long)item.GroupId,
            TeacherId = (long)item.TeacherId,
            item.CreationTime
        };

        const string Query =
            $"""
             insert into {GroupTeacherTable.TableName} (
                         {GroupTeacherTable.GroupId},
                         {GroupTeacherTable.TeacherId},
                         {GroupTeacherTable.CreationTime})
                  values (
                         @{nameof(queryParams.GroupId)},
                         @{nameof(queryParams.TeacherId)},
                         @{nameof(queryParams.CreationTime)});
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        await _connectionContext.Connection.ExecuteAsync(command);
    }

    public async Task<IReadOnlyCollection<TeacherId>> ListTeacherIdAsync(GroupId groupId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            GroupId = (long)groupId
        };

        const string Query =
            $"""
             select {GroupTeacherTable.TeacherId}
               from {GroupTeacherTable.TableName}
              where {GroupTeacherTable.GroupId} = @{nameof(queryParams.GroupId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var teacherIds = await _connectionContext.Connection.QueryAsync<long>(command);

        return teacherIds.ToArrayBy(teacherId => new TeacherId(teacherId));
    }

    public async Task<bool> ExistsAsync(GroupTeacher groupTeacher, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            GroupId = (long)groupTeacher.GroupId,
            TeacherId = (long)groupTeacher.TeacherId
        };

        const string Query =
            $"""
             select exists(select
                             from {GroupTeacherTable.TableName}
                            where {GroupTeacherTable.GroupId} = @{nameof(queryParams.GroupId)}
                              and {GroupTeacherTable.TeacherId} = @{nameof(queryParams.TeacherId)});
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        return await _connectionContext.Connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<IReadOnlyCollection<GroupTeacher>> ListAsync(GroupTeacherFilter filter, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            TeacherIds = filter.TeacherIds.ToArrayBy(teacherId => (long)teacherId),
            GroupIds = filter.GroupIds.ToArrayBy(groupId => (long)groupId)
        };

        const string Query =
            $"""
             select {GroupTeacherTable.GroupId},
                    {GroupTeacherTable.TeacherId}
               from {GroupTeacherTable.TableName}
              where (cardinality(@{nameof(queryParams.TeacherIds)}) = 0
                    or {GroupTeacherTable.TeacherId} = any(@{nameof(queryParams.TeacherIds)}))
                and (cardinality(@{nameof(queryParams.GroupIds)}) = 0
                    or {GroupTeacherTable.GroupId} = any(@{nameof(queryParams.GroupIds)}));
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var groupTeacherDbs = await _connectionContext.Connection.QueryAsync<GroupTeacherDb>(command);

        return groupTeacherDbs.ToArrayBy(groupTeacherDb => groupTeacherDb.ToGroupTeacher());
    }
}
