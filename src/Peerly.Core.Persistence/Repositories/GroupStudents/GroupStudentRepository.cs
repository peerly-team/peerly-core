using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
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
}
