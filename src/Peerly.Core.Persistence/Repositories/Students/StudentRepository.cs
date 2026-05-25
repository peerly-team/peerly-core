using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Students;
using Peerly.Core.Persistence.Common;
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

    public async Task<Student?> GetAsync(StudentId studentId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            StudentId = (long)studentId
        };

        const string Query =
            $"""
             select {StudentTable.Id},
                    {StudentTable.Email},
                    {StudentTable.Name}
               from {StudentTable.TableName}
              where {StudentTable.Id} = @{nameof(queryParams.StudentId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var result = await _connectionContext.Connection.QuerySingleOrDefaultAsync<StudentDb>(command);

        return result?.ToStudent();
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

    public async Task<bool> UpdateAsync(
        StudentId studentId,
        Action<IUpdateBuilder<StudentUpdateItem>> configureUpdate,
        CancellationToken cancellationToken)
    {
        var builder = new UpdateBuilder<StudentUpdateItem>();
        configureUpdate(builder);

        var configuration = builder.Build();
        var queryParams = configuration.GetQueryParams();
        queryParams.Add($"@{nameof(studentId)}", (long)studentId);

        var query =
            $"""
             update {StudentTable.TableName} as new
                set {StudentTable.UpdateTime} = now(),
                    {StudentTable.Name} = case
                    when {configuration.GetFlagParamName(item => item.Name)}
                    then {configuration.GetParamName(item => item.Name)}
                    else {StudentTable.Name}
                    end
              from (select {StudentTable.Id}
                      from {StudentTable.TableName}
                     where {StudentTable.Id} = @{nameof(studentId)}
                       for update) as old
             WHERE new.{StudentTable.Id} = old.{StudentTable.Id};
             """;

        var command = new CommandDefinition(
            query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var affectedRows = await _connectionContext.Connection.ExecuteAsync(command);

        return affectedRows == 1;
    }
}
