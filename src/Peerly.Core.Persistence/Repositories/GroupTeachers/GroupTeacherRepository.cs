using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
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
}
