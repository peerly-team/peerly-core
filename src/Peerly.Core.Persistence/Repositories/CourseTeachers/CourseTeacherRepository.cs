using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.CourseTeachers;

internal sealed class CourseTeacherRepository : ICourseTeacherRepository
{
    private readonly IConnectionContext _connectionContext;

    public CourseTeacherRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<bool> AddAsync(CourseTeacherAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseId = (long)item.CourseId,
            TeacherId = (long)item.TeacherId,
            item.CreationTime
        };

        const string Query =
            $"""
             insert into {CourseTeacherTable.TableName} (
                         {CourseTeacherTable.CourseId},
                         {CourseTeacherTable.TeacherId},
                         {CourseTeacherTable.CreationTime})
                  values (
                         @{nameof(queryParams.CourseId)},
                         @{nameof(queryParams.TeacherId)},
                         @{nameof(queryParams.CreationTime)});
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var affectedRows = await _connectionContext.Connection.ExecuteAsync(command);

        return affectedRows == 1;
    }

    public async Task<IReadOnlyCollection<CourseId>> ListCourseIdAsync(TeacherId teacherId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            TeacherId = (long)teacherId
        };

        const string Query =
            $"""
             select {CourseTeacherTable.CourseId}
               from {CourseTeacherTable.TableName}
              where {CourseTeacherTable.TeacherId} = @{nameof(queryParams.TeacherId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var courseIds = await _connectionContext.Connection.QueryAsync<long>(command);

        return courseIds.ToArrayBy(courseId => new CourseId(courseId));
    }

    public async Task<IReadOnlyCollection<TeacherId>> ListTeacherIdAsync(CourseId courseId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseId = (long)courseId
        };

        const string Query =
            $"""
             select {CourseTeacherTable.TeacherId}
               from {CourseTeacherTable.TableName}
              where {CourseTeacherTable.CourseId} = @{nameof(queryParams.CourseId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var teacherIds = await _connectionContext.Connection.QueryAsync<long>(command);

        return teacherIds.ToArrayBy(teacherId => new TeacherId(teacherId));
    }

    public async Task<bool> ExistsAsync(CourseTeacherExistsItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseId = (long)item.CourseId,
            TeacherId = (long)item.TeacherId
        };

        const string Query =
            $"""
              select exists(select
                              from {CourseTeacherTable.TableName}
                             where {CourseTeacherTable.CourseId} = @{nameof(queryParams.CourseId)}
                               and {CourseTeacherTable.TeacherId} = @{nameof(queryParams.TeacherId)});
              """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        return await _connectionContext.Connection.ExecuteScalarAsync<bool>(command);
    }
}
