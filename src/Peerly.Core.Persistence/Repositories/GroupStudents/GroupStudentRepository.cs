using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;
using Peerly.Core.Persistence.Repositories.GroupStudents.Models;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.GroupStudents;

internal sealed class GroupStudentRepository : IGroupStudentRepository
{
    private readonly IConnectionContext _connectionContext;

    public GroupStudentRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task AddAsync(GroupStudentAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            GroupId = (long)item.GroupId,
            StudentId = (long)item.StudentId,
            item.CreationTime
        };

        const string Query =
            $"""
             insert into {GroupStudentTable.TableName} (
                         {GroupStudentTable.GroupId},
                         {GroupStudentTable.StudentId},
                         {GroupStudentTable.CreationTime})
                  values (
                         @{nameof(queryParams.GroupId)},
                         @{nameof(queryParams.StudentId)},
                         @{nameof(queryParams.CreationTime)});
             """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        await _connectionContext.Connection.ExecuteAsync(command);
    }

    public async Task<IReadOnlyCollection<StudentId>> BulkAddAsync(GroupStudentBulkAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            GroupId = (long)item.GroupId,
            StudentIds = item.StudentIds.ToArrayBy(studentId => (long)studentId),
            item.CreationTime
        };

        const string Query =
            $"""
             insert into {GroupStudentTable.TableName} (
                         {GroupStudentTable.GroupId},
                         {GroupStudentTable.StudentId},
                         {GroupStudentTable.CreationTime})
                  select @{nameof(queryParams.GroupId)},
                         {GroupStudentTable.StudentId},
                         @{nameof(queryParams.CreationTime)}
                    from unnest(@{nameof(queryParams.StudentIds)}) as {GroupStudentTable.StudentId}
             on conflict ({GroupStudentTable.GroupId}, {GroupStudentTable.StudentId}) do nothing
               returning {GroupStudentTable.StudentId};
             """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var studentIds = await _connectionContext.Connection.QueryAsync<long>(command);

        return studentIds.ToArrayBy(studentId => new StudentId(studentId));
    }

    public async Task<bool> ExistsAsync(GroupStudent groupStudent, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            GroupId = (long)groupStudent.GroupId,
            StudentId = (long)groupStudent.StudentId
        };

        const string Query =
            $"""
             select exists(select
                             from {GroupStudentTable.TableName}
                            where {GroupStudentTable.GroupId} = @{nameof(queryParams.GroupId)}
                              and {GroupStudentTable.StudentId} = @{nameof(queryParams.StudentId)});
             """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        return await _connectionContext.Connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<bool> ExistsAsync(CourseStudent courseStudent, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseId = (long)courseStudent.CourseId,
            StudentId = (long)courseStudent.StudentId
        };

        const string Query =
            $"""
             select exists (
                 select
                   from {GroupStudentTable.TableName} gs
                   join {GroupTable.TableName} g on g.{GroupTable.Id} = gs.{GroupStudentTable.GroupId}
                  where g.{GroupTable.CourseId} = @{nameof(queryParams.CourseId)}
                    and gs.{GroupStudentTable.StudentId} = @{nameof(queryParams.StudentId)});
             """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        return await _connectionContext.Connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<IReadOnlyCollection<GroupStudent>> ListAsync(GroupStudentFilter filter, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            StudentIds = filter.StudentIds.ToArrayBy(studentId => (long)studentId),
            GroupIds = filter.GroupIds.ToArrayBy(groupId => (long)groupId)
        };

        const string Query =
            $"""
             select {GroupStudentTable.GroupId},
                    {GroupStudentTable.StudentId}
               from {GroupStudentTable.TableName}
              where (cardinality(@{nameof(queryParams.StudentIds)}) = 0
                    or {GroupStudentTable.StudentId} = any(@{nameof(queryParams.StudentIds)}))
                and (cardinality(@{nameof(queryParams.GroupIds)}) = 0
                    or {GroupStudentTable.GroupId} = any(@{nameof(queryParams.GroupIds)}));
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var groupStudentDbs = await _connectionContext.Connection.QueryAsync<GroupStudentDb>(command);

        return groupStudentDbs.ToArrayBy(groupStudentDb => groupStudentDb.ToGroupStudent());
    }

    public async Task DeleteByGroupAsync(GroupId groupId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            GroupId = (long)groupId
        };

        const string Query =
            $"""
             delete from {GroupStudentTable.TableName}
                   where {GroupStudentTable.GroupId} = @{nameof(queryParams.GroupId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        await _connectionContext.Connection.ExecuteAsync(command);
    }
}
