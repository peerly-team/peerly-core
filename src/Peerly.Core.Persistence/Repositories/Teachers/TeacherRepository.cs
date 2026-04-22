using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Models.Teachers;
using Peerly.Core.Persistence.Repositories.Teachers.Models;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.Teachers;

internal sealed class TeacherRepository : ITeacherRepository
{
    private readonly IConnectionContext _connectionContext;

    public TeacherRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<IReadOnlyCollection<Teacher>> ListAsync(TeacherFilter filter, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            TeacherIds = filter.TeacherIds.ToArrayBy(teacherId => (long)teacherId)
        };

        const string Query =
            $"""
             select {TeacherTable.Id},
                    {TeacherTable.Email},
                    {TeacherTable.Name}
               from {TeacherTable.TableName}
              where cardinality(@{nameof(queryParams.TeacherIds)}) = 0
                 or {TeacherTable.Id} = any(@{nameof(queryParams.TeacherIds)});
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var results = await _connectionContext.Connection.QueryAsync<TeacherDb>(command);

        return results.ToArrayBy(db => db.ToTeacher());
    }

    public async Task<bool> AddIfNotExistsAsync(TeacherAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            Id = (long)item.Id,
            item.Email,
            item.Name,
            item.CreationTime
        };

        const string Query =
            $"""
             insert into {TeacherTable.TableName} (
                         {TeacherTable.Id},
                         {TeacherTable.Email},
                         {TeacherTable.Name},
                         {TeacherTable.CreationTime})
                  values (
                         @{nameof(queryParams.Id)},
                         @{nameof(queryParams.Email)},
                         @{nameof(queryParams.Name)},
                         @{nameof(queryParams.CreationTime)})
             on conflict ({TeacherTable.Id}) do nothing;
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var affectedRows = await _connectionContext.Connection.ExecuteAsync(command);

        return affectedRows == 1;
    }
}
