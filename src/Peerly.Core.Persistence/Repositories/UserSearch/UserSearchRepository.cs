using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Models.Users;
using Peerly.Core.Persistence.Repositories.UserSearch.Models;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.UserSearch;

internal sealed class UserSearchRepository : IReadOnlyUserSearchRepository
{
    private readonly IConnectionContext _connectionContext;

    public UserSearchRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<IReadOnlyCollection<User>> ListAsync(UserFilter filter, CancellationToken cancellationToken)
    {
        if (filter.Roles.Count == 0)
        {
            return [];
        }

        var queryParams = new
        {
            filter.Query,
            filter.Limit
        };

        var includeStudents = filter.Roles.Contains(UserRole.Student);
        var includeTeachers = filter.Roles.Contains(UserRole.Teacher);

        var sql = new StringBuilder();

        if (includeStudents)
        {
            sql.Append(
                $"""
                 SELECT {StudentTable.Id}, {StudentTable.Email}, {StudentTable.Name}, 'Student' AS role
                   FROM {StudentTable.TableName}
                  WHERE {StudentTable.Email} ILIKE @{nameof(queryParams.Query)} || '%'
                     OR {StudentTable.Name} ILIKE '%' || @{nameof(queryParams.Query)} || '%'
                 """);
        }

        if (includeStudents && includeTeachers)
        {
            sql.AppendLine();
            sql.AppendLine("UNION ALL");
        }

        if (includeTeachers)
        {
            sql.Append(
                $"""
                 SELECT {TeacherTable.Id}, {TeacherTable.Email}, {TeacherTable.Name}, 'Teacher' AS role
                   FROM {TeacherTable.TableName}
                  WHERE {TeacherTable.Email} ILIKE @{nameof(queryParams.Query)} || '%'
                     OR {TeacherTable.Name} ILIKE '%' || @{nameof(queryParams.Query)} || '%'
                 """);
        }

        sql.AppendLine();
        sql.Append($"ORDER BY name LIMIT @{nameof(queryParams.Limit)}");

        var command = new CommandDefinition(
            commandText: sql.ToString(),
            queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var results = await _connectionContext.Connection.QueryAsync<UserDb>(command);

        return results.ToArrayBy(db => db.ToUser());
    }
}
