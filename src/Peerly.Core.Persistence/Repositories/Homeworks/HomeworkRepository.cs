using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Persistence.Common;
using Peerly.Core.Persistence.Repositories.Homeworks.Models;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.Homeworks;

internal sealed class HomeworkRepository : IHomeworkRepository
{
    private readonly IConnectionContext _connectionContext;

    public HomeworkRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<Homework?> GetAsync(HomeworkId homeworkId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            HomeworkId = (long)homeworkId
        };

        const string Query =
            $"""
             select {HomeworkTable.Id},
                    {HomeworkTable.CourseId},
                    {HomeworkTable.GroupId},
                    {HomeworkTable.TeacherId},
                    {HomeworkTable.Name},
                    {HomeworkTable.Status},
                    {HomeworkTable.AmountOfReviewers},
                    {HomeworkTable.Description},
                    {HomeworkTable.Checklist},
                    {HomeworkTable.Deadline},
                    {HomeworkTable.ReviewDeadline},
                    {HomeworkTable.DiscrepancyThreshold}
               from {HomeworkTable.TableName}
              where {HomeworkTable.Id} = @{nameof(queryParams.HomeworkId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var homeworkDb = await _connectionContext.Connection.QuerySingleOrDefaultAsync<HomeworkDb>(command);

        return homeworkDb?.ToHomework();
    }

    public async Task<int> GetHomeworkCountAsync(CourseId courseId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseId = (long)courseId
        };

        const string Query =
            $"""
             select count(*)
               from {HomeworkTable.TableName}
              where {HomeworkTable.CourseId} = @{nameof(queryParams.CourseId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        return await _connectionContext.Connection.ExecuteScalarAsync<int>(command);
    }

    public async Task<IReadOnlyCollection<Homework>> ListAsync(HomeworkFilter filter, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseIds = filter.CourseIds.ToArrayBy(courseId => (long)courseId),
            GroupIds = filter.GroupIds.ToArrayBy(groupId => (long)groupId),
            HomeworkStatuses = filter.HomeworkStatuses.ToArrayBy(homeworkStatus => homeworkStatus.ToString())
        };

        const string Query =
            $"""
             select {HomeworkTable.Id},
                    {HomeworkTable.CourseId},
                    {HomeworkTable.GroupId},
                    {HomeworkTable.TeacherId},
                    {HomeworkTable.Name},
                    {HomeworkTable.Status},
                    {HomeworkTable.AmountOfReviewers},
                    {HomeworkTable.Description},
                    {HomeworkTable.Checklist},
                    {HomeworkTable.Deadline},
                    {HomeworkTable.ReviewDeadline},
                    {HomeworkTable.DiscrepancyThreshold}
               from {HomeworkTable.TableName}
              where (cardinality(@{nameof(queryParams.CourseIds)}) = 0
                    or {HomeworkTable.CourseId} = any(@{nameof(queryParams.CourseIds)}))
                and (cardinality(@{nameof(queryParams.HomeworkStatuses)}) = 0
                    or {HomeworkTable.Status} = any(@{nameof(queryParams.HomeworkStatuses)}))
                and (cardinality(@{nameof(queryParams.GroupIds)}) = 0
                    or {HomeworkTable.GroupId} = any(@{nameof(queryParams.GroupIds)}));
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var homeworkDbs = await _connectionContext.Connection.QueryAsync<HomeworkDb>(command);

        return homeworkDbs.ToArrayBy(homeworkDb => homeworkDb.ToHomework());
    }

    public async Task<HomeworkId> AddAsync(HomeworkAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseId = (long)item.CourseId,
            GroupId = (long?)item.GroupId,
            TeacherId = (long)item.TeacherId,
            item.Name,
            Status = item.Status.ToString(),
            item.AmountOfReviewers,
            item.Description,
            item.Checklist,
            item.Deadline,
            item.ReviewDeadline,
            item.DiscrepancyThreshold,
            item.CreationTime
        };

        const string Query =
            $"""
             insert into {HomeworkTable.TableName} (
                         {HomeworkTable.CourseId},
                         {HomeworkTable.GroupId},
                         {HomeworkTable.TeacherId},
                         {HomeworkTable.Name},
                         {HomeworkTable.Status},
                         {HomeworkTable.AmountOfReviewers},
                         {HomeworkTable.Description},
                         {HomeworkTable.Checklist},
                         {HomeworkTable.Deadline},
                         {HomeworkTable.ReviewDeadline},
                         {HomeworkTable.DiscrepancyThreshold},
                         {HomeworkTable.CreationTime})
                  values (
                         @{nameof(queryParams.CourseId)},
                         @{nameof(queryParams.GroupId)},
                         @{nameof(queryParams.TeacherId)},
                         @{nameof(queryParams.Name)},
                         @{nameof(queryParams.Status)},
                         @{nameof(queryParams.AmountOfReviewers)},
                         @{nameof(queryParams.Description)},
                         @{nameof(queryParams.Checklist)},
                         @{nameof(queryParams.Deadline)},
                         @{nameof(queryParams.ReviewDeadline)},
                         @{nameof(queryParams.DiscrepancyThreshold)},
                         @{nameof(queryParams.CreationTime)})
               returning {HomeworkTable.Id};
             """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var homeworkId = await _connectionContext.Connection.QuerySingleAsync<long>(command);

        return new HomeworkId(homeworkId);
    }

    public async Task<bool> UpdateAsync(
        HomeworkId homeworkId,
        Action<IUpdateBuilder<HomeworkUpdateItem>> configureUpdate,
        CancellationToken cancellationToken)
    {
        var builder = new UpdateBuilder<HomeworkUpdateItem>();
        configureUpdate(builder);

        var configuration = builder.Build();
        var queryParams = configuration.GetQueryParams();
        queryParams.Add($"@{nameof(homeworkId)}", (long)homeworkId);

        var query =
            $"""
             update {HomeworkTable.TableName} as new
                set {HomeworkTable.UpdateTime} = now(),
                    {HomeworkTable.Name} = case
                    when {configuration.GetFlagParamName(item => item.Name)}
                    then {configuration.GetParamName(item => item.Name)}
                    else {HomeworkTable.Name}
                    end,
                    {HomeworkTable.Description} = case
                    when {configuration.GetFlagParamName(item => item.Description)}
                    then {configuration.GetParamName(item => item.Description)}
                    else {HomeworkTable.Description}
                    end,
                    {HomeworkTable.Status} = case
                    when {configuration.GetFlagParamName(item => item.Status)}
                    then {configuration.GetParamName(item => item.Status)}
                    else {HomeworkTable.Status}
                    end,
                    {HomeworkTable.Checklist} = case
                    when {configuration.GetFlagParamName(item => item.Checklist)}
                    then {configuration.GetParamName(item => item.Checklist)}
                    else {HomeworkTable.Checklist}
                    end,
                    {HomeworkTable.AmountOfReviewers} = case
                    when {configuration.GetFlagParamName(item => item.AmountOfReviewers)}
                    then {configuration.GetParamName(item => item.AmountOfReviewers)}
                    else {HomeworkTable.AmountOfReviewers}
                    end,
                    {HomeworkTable.Deadline} = case
                    when {configuration.GetFlagParamName(item => item.Deadline)}
                    then {configuration.GetParamName(item => item.Deadline)}
                    else {HomeworkTable.Deadline}
                    end,
                    {HomeworkTable.ReviewDeadline} = case
                    when {configuration.GetFlagParamName(item => item.ReviewDeadline)}
                    then {configuration.GetParamName(item => item.ReviewDeadline)}
                    else {HomeworkTable.ReviewDeadline}
                    end,
                    {HomeworkTable.DiscrepancyThreshold} = case
                    when {configuration.GetFlagParamName(item => item.DiscrepancyThreshold)}
                    then {configuration.GetParamName(item => item.DiscrepancyThreshold)}
                    else {HomeworkTable.DiscrepancyThreshold}
                    end
              from (select {HomeworkTable.Id}
                      from {HomeworkTable.TableName}
                     where {HomeworkTable.Id} = @{nameof(homeworkId)}
                       for update) as old
             WHERE new.{HomeworkTable.Id} = old.{HomeworkTable.Id};
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
