using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Pagination;
using Peerly.Core.Persistence.Common;
using Peerly.Core.Persistence.Repositories.Courses.Models;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.Courses;

internal sealed class CourseRepository : ICourseRepository
{
    private readonly IConnectionContext _connectionContext;

    public CourseRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<CourseId> AddAsync(CourseAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            item.Name,
            item.Description,
            Status = item.Status.ToString(),
            item.CreationTime
        };

        const string Query =
            $"""
             insert into {CourseTable.TableName} (
                         {CourseTable.Name},
                         {CourseTable.Description},
                         {CourseTable.Status},
                         {CourseTable.CreationTime})
                  values (
                         @{nameof(queryParams.Name)},
                         @{nameof(queryParams.Description)},
                         @{nameof(queryParams.Status)},
                         @{nameof(queryParams.CreationTime)})
               returning {CourseTable.Id};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var id = await _connectionContext.Connection.QuerySingleAsync<long>(command);

        return new CourseId(id);
    }

    public async Task<bool> UpdateAsync(
        CourseId courseId,
        Action<IUpdateBuilder<CourseUpdateItem>> configureUpdate,
        CancellationToken cancellationToken)
    {
        var builder = new UpdateBuilder<CourseUpdateItem>();
        configureUpdate(builder);

        var configuration = builder.Build();
        var queryParams = configuration.GetQueryParams();
        queryParams.Add($"@{nameof(courseId)}", (long)courseId);

        var query =
            $"""
             update {CourseTable.TableName} as new
                set {CourseTable.UpdateTime} = now(),
                    {CourseTable.Name} = case
                    when {configuration.GetFlagParamName(item => item.Name)}
                    then {configuration.GetParamName(item => item.Name)}
                    else {CourseTable.Name}
                    end,
                    {CourseTable.Description} = case
                    when {configuration.GetFlagParamName(item => item.Description)}
                    then {configuration.GetParamName(item => item.Description)}
                    else {CourseTable.Description}
                    end,
                    {CourseTable.Status} = case
                    when {configuration.GetFlagParamName(item => item.Status)}
                    then {configuration.GetParamName(item => item.Status)}
                    else {CourseTable.Status}
                    end
              from (select {CourseTable.Id}
                      from {CourseTable.TableName}
                     where {CourseTable.Id} = @{nameof(courseId)}
                       for update) as old
             WHERE new.{CourseTable.Id} = old.{CourseTable.Id};
             """;

        var command = new CommandDefinition(
            query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        var affectedRows = await _connectionContext.Connection.ExecuteAsync(command);

        return affectedRows == 1;
    }

    public async Task<Course?> GetAsync(CourseId courseId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseId = (long)courseId
        };

        const string Query =
            $"""
             select {CourseTable.Id},
                    {CourseTable.Name},
                    {CourseTable.Description},
                    {CourseTable.Status}
               from {CourseTable.TableName}
              where {CourseTable.Id} = @{nameof(queryParams.CourseId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var courseDb = await _connectionContext.Connection.QuerySingleOrDefaultAsync<CourseDb>(command);

        return courseDb.ToCourse();
    }

    public async Task<IReadOnlyCollection<Course>> ListAsync(
        CourseFilter filter,
        PaginationInfo paginationInfo,
        CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseIds = filter.CourseIds.ToArrayBy(courseId => (long)courseId),
            CourseStatuses = filter.CourseStatuses.ToArrayBy(courseStatus => courseStatus.ToString()),
            Limit = paginationInfo.PageSize,
            paginationInfo.Offset,
        };

        const string Query =
            $"""
             select {CourseTable.Id},
                    {CourseTable.Name},
                    {CourseTable.Description},
                    {CourseTable.Status}
               from {CourseTable.TableName}
              where (cardinality(@{nameof(queryParams.CourseIds)}) = 0
                    or {CourseTable.Id} = any(@{nameof(queryParams.CourseIds)}))
                and (cardinality(@{nameof(queryParams.CourseStatuses)}) = 0
                    or {CourseTable.Status} = any(@{nameof(queryParams.CourseStatuses)}))
              limit @{nameof(queryParams.Limit)}
             offset @{nameof(queryParams.Offset)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var courseDbs = await _connectionContext.Connection.QueryAsync<CourseDb>(command);

        return courseDbs.ToCourses();
    }
}
