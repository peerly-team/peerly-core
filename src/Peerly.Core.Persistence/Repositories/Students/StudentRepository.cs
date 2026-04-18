using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Models.Students;
using Peerly.Core.Persistence.Repositories.Students.Models;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.Students;

internal sealed class StudentRepository : IStudentRepository
{
    private readonly IConnectionContext _connectionContext;

    public StudentRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<IReadOnlyCollection<Student>> ListAsync(StudentFilter filter, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            StudentIds = filter.StudentIds.ToArrayBy(studentId => (long)studentId)
        };

        const string Query =
            $"""
             select {StudentTable.Id},
                    {StudentTable.Email},
                    {StudentTable.Name}
               from {StudentTable.TableName}
              where cardinality(@{nameof(queryParams.StudentIds)}) = 0
                 or {StudentTable.Id} = any(@{nameof(queryParams.StudentIds)});
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var results = await _connectionContext.Connection.QueryAsync<StudentDb>(command);

        return results.ToArrayBy(db => db.ToStudent());
    }

    public async Task<bool> AddIfNotExistsAsync(StudentAddItem item, CancellationToken cancellationToken)
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
             insert into {StudentTable.TableName} (
                         {StudentTable.Id},
                         {StudentTable.Email},
                         {StudentTable.Name},
                         {StudentTable.CreationTime})
                  values (
                         @{nameof(queryParams.Id)},
                         @{nameof(queryParams.Email)},
                         @{nameof(queryParams.Name)},
                         @{nameof(queryParams.CreationTime)})
             on conflict ({StudentTable.Id}) do nothing;
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
