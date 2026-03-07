using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
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

    public async Task<IReadOnlyCollection<GroupId>> ListGroupIdAsync(StudentId studentId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            StudentId = studentId
        };

        const string Query =
            $"""
             select {GroupStudentTable.GroupId}
               from {GroupStudentTable.TableName}
              where {GroupStudentTable.StudentId} = @{nameof(queryParams.StudentId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var groupIds = await _connectionContext.Connection.QueryAsync<long>(command);

        return groupIds.ToArrayBy(groupId => new GroupId(groupId));
    }
}
