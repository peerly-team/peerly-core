using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Pagination;
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

    public async Task<StudentHomeworkInfo?> GetStudentHomeworkInfoAsync(HomeworkStudent homeworkStudent, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            HomeworkId = (long)homeworkStudent.HomeworkId,
            StudentId = (long)homeworkStudent.StudentId
        };

        const string Query =
            $"""
             select h.{HomeworkTable.Id},
                    h.{HomeworkTable.Name},
                    h.{HomeworkTable.Status},
                    h.{HomeworkTable.AmountOfReviewers},
                    h.{HomeworkTable.Description},
                    h.{HomeworkTable.Checklist},
                    h.{HomeworkTable.Deadline},
                    h.{HomeworkTable.ReviewDeadline},
                    (sh.{SubmittedHomeworkTable.HomeworkId} is not null) as is_homework_submitted
               from {HomeworkTable.TableName} h
               left join {SubmittedHomeworkTable.TableName} sh
                 on sh.{SubmittedHomeworkTable.HomeworkId} = h.{HomeworkTable.Id}
                and sh.{SubmittedHomeworkTable.StudentId} = @{nameof(queryParams.StudentId)}
              where h.{HomeworkTable.Id} = @{nameof(queryParams.HomeworkId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var homeworkDb = await _connectionContext.Connection.QuerySingleOrDefaultAsync<StudentHomeworkInfoDb>(command);

        return homeworkDb?.ToStudentHomeworkInfo();
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

    public async Task<TeacherHomeworkInfo?> GetTeacherHomeworkInfoAsync(HomeworkId homeworkId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            HomeworkId = (long)homeworkId
        };

        const string Query =
            $"""
             select h.{HomeworkTable.Id},
                    h.{HomeworkTable.Name},
                    h.{HomeworkTable.Status},
                    h.{HomeworkTable.AmountOfReviewers},
                    h.{HomeworkTable.Description},
                    h.{HomeworkTable.Checklist},
                    h.{HomeworkTable.Deadline},
                    h.{HomeworkTable.ReviewDeadline},
                    h.{HomeworkTable.DiscrepancyThreshold}
               from {HomeworkTable.TableName} h
              where h.{HomeworkTable.Id} = @{nameof(queryParams.HomeworkId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var homeworkDb = await _connectionContext.Connection.QuerySingleOrDefaultAsync<TeacherHomeworkInfoDb>(command);

        return homeworkDb?.ToTeacherHomeworkInfo();
    }

    public async Task<IReadOnlyCollection<StudentHomeworkInfo>> ListStudentHomeworkInfosAsync(
        CourseStudent courseStudent,
        CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseId = (long)courseStudent.CourseId,
            StudentId = (long)courseStudent.StudentId,
            DraftStatus = HomeworkStatus.Draft.ToString()
        };

        const string Query =
            $"""
              with student_course_groups as (
                  select gs.{GroupStudentTable.GroupId}
                    from {GroupStudentTable.TableName} gs
                    join {GroupTable.TableName} g on g.{GroupTable.Id} = gs.{GroupStudentTable.GroupId}
                   where g.{GroupTable.CourseId} = @{nameof(queryParams.CourseId)}
                     and gs.{GroupStudentTable.StudentId} = @{nameof(queryParams.StudentId)})

              select h.{HomeworkTable.Id},
                     h.{HomeworkTable.Name},
                     h.{HomeworkTable.Status},
                     h.{HomeworkTable.AmountOfReviewers},
                     h.{HomeworkTable.Description},
                     h.{HomeworkTable.Checklist},
                     h.{HomeworkTable.Deadline},
                     h.{HomeworkTable.ReviewDeadline},
                     (sh.{SubmittedHomeworkTable.HomeworkId} is not null) as is_homework_submitted
                from {HomeworkTable.TableName} h
                left join {SubmittedHomeworkTable.TableName} sh on sh.{SubmittedHomeworkTable.HomeworkId} = h.{HomeworkTable.Id}
                                                                and sh.{SubmittedHomeworkTable.StudentId} = @{nameof(queryParams.StudentId)}
               where h.{HomeworkTable.Status} <> @{nameof(queryParams.DraftStatus)}
                 and h.{HomeworkTable.CourseId} = @{nameof(queryParams.CourseId)}
                 and (
                     (h.{HomeworkTable.GroupId} is null and exists (select from student_course_groups))
                     or h.{HomeworkTable.GroupId} in (select group_id from student_course_groups)
                     )
               order by h.{HomeworkTable.Deadline} desc;
              """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var homeworkDbs = await _connectionContext.Connection.QueryAsync<StudentHomeworkInfoDb>(command);

        return homeworkDbs.ToArrayBy(homeworkDb => homeworkDb.ToStudentHomeworkInfo());
    }

    public async Task<IReadOnlyCollection<TeacherHomeworkInfo>> ListTeacherHomeworkInfosAsync(
        CourseTeacher courseTeacher,
        CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseId = (long)courseTeacher.CourseId,
            TeacherId = (long)courseTeacher.TeacherId
        };

        const string Query =
            $"""
             select h.{HomeworkTable.Id},
                    h.{HomeworkTable.Name},
                    h.{HomeworkTable.Status},
                    h.{HomeworkTable.AmountOfReviewers},
                    h.{HomeworkTable.Description},
                    h.{HomeworkTable.Checklist},
                    h.{HomeworkTable.Deadline},
                    h.{HomeworkTable.ReviewDeadline},
                    h.{HomeworkTable.DiscrepancyThreshold}
               from {HomeworkTable.TableName} h
              where h.{HomeworkTable.CourseId} = @{nameof(queryParams.CourseId)}
                and (
                    exists (select
                              from {CourseTeacherTable.TableName} ct
                             where ct.{CourseTeacherTable.CourseId} = @{nameof(queryParams.CourseId)}
                               and ct.{CourseTeacherTable.TeacherId} = @{nameof(queryParams.TeacherId)})
                    or exists (select 1
                                 from {GroupTeacherTable.TableName} gt
                                 join {GroupTable.TableName} g on g.{GroupTable.Id} = gt.{GroupTeacherTable.GroupId}
                                where g.{GroupTable.CourseId} = @{nameof(queryParams.CourseId)}
                                  and gt.{GroupTeacherTable.TeacherId} = @{nameof(queryParams.TeacherId)})
                    )
              order by h.{HomeworkTable.Deadline} desc;
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var homeworkDbs = await _connectionContext.Connection.QueryAsync<TeacherHomeworkInfoDb>(command);

        return homeworkDbs.ToArrayBy(homeworkDb => homeworkDb.ToTeacherHomeworkInfo());
    }

    public async Task<IReadOnlyCollection<TeacherHomeworkInfo>> SearchTeacherHomeworkInfosAsync(
        TeacherHomeworkSearchFilter filter,
        PaginationInfo paginationInfo,
        CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseIds = filter.CourseIds.ToArrayBy(courseId => (long)courseId),
            GroupIds = filter.GroupIds.ToArrayBy(groupId => (long)groupId),
            HomeworkStatuses = filter.HomeworkStatuses.ToArrayBy(homeworkStatus => homeworkStatus.ToString()),
            Limit = paginationInfo.PageSize,
            paginationInfo.Offset
        };

        const string Query =
            $"""
             select h.{HomeworkTable.Id},
                    h.{HomeworkTable.Name},
                    h.{HomeworkTable.Status},
                    h.{HomeworkTable.AmountOfReviewers},
                    h.{HomeworkTable.Description},
                    h.{HomeworkTable.Checklist},
                    h.{HomeworkTable.Deadline},
                    h.{HomeworkTable.ReviewDeadline},
                    h.{HomeworkTable.DiscrepancyThreshold}
               from {HomeworkTable.TableName} h
              where (h.{HomeworkTable.CourseId} = any(@{nameof(queryParams.CourseIds)})
                    or h.{HomeworkTable.GroupId} = any(@{nameof(queryParams.GroupIds)}))
                and (cardinality(@{nameof(queryParams.HomeworkStatuses)}) = 0
                    or h.{HomeworkTable.Status} = any(@{nameof(queryParams.HomeworkStatuses)}))
              order by h.{HomeworkTable.Deadline} desc
              limit @{nameof(queryParams.Limit)}
             offset @{nameof(queryParams.Offset)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var homeworkDbs = await _connectionContext.Connection.QueryAsync<TeacherHomeworkInfoDb>(command);

        return homeworkDbs.ToArrayBy(homeworkDb => homeworkDb.ToTeacherHomeworkInfo());
    }

    public async Task<IReadOnlyCollection<StudentHomeworkInfo>> SearchStudentHomeworkInfosAsync(
        StudentHomeworkSearchFilter filter,
        PaginationInfo paginationInfo,
        CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            StudentId = (long)filter.StudentId,
            CourseIds = filter.CourseIds.ToArrayBy(courseId => (long)courseId),
            GroupIds = filter.GroupIds.ToArrayBy(groupId => (long)groupId),
            HomeworkStatuses = filter.HomeworkStatuses.ToArrayBy(homeworkStatus => homeworkStatus.ToString()),
            Limit = paginationInfo.PageSize,
            paginationInfo.Offset
        };

        const string Query =
            $"""
             select h.{HomeworkTable.Id},
                    h.{HomeworkTable.Name},
                    h.{HomeworkTable.Status},
                    h.{HomeworkTable.AmountOfReviewers},
                    h.{HomeworkTable.Description},
                    h.{HomeworkTable.Checklist},
                    h.{HomeworkTable.Deadline},
                    h.{HomeworkTable.ReviewDeadline},
                    (sh.{SubmittedHomeworkTable.HomeworkId} is not null) as is_homework_submitted
               from {HomeworkTable.TableName} h
               left join {SubmittedHomeworkTable.TableName} sh on sh.{SubmittedHomeworkTable.HomeworkId} = h.{HomeworkTable.Id}
                                                               and sh.{SubmittedHomeworkTable.StudentId} = @{nameof(queryParams.StudentId)}
              where (h.{HomeworkTable.CourseId} = any(@{nameof(queryParams.CourseIds)})
                    or h.{HomeworkTable.GroupId} = any(@{nameof(queryParams.GroupIds)}))
                and (cardinality(@{nameof(queryParams.HomeworkStatuses)}) = 0
                    or h.{HomeworkTable.Status} = any(@{nameof(queryParams.HomeworkStatuses)}))
              order by h.{HomeworkTable.Deadline} desc
              limit @{nameof(queryParams.Limit)}
             offset @{nameof(queryParams.Offset)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var homeworkDbs = await _connectionContext.Connection.QueryAsync<StudentHomeworkInfoDb>(command);

        return homeworkDbs.ToArrayBy(homeworkDb => homeworkDb.ToStudentHomeworkInfo());
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

    public async Task DeleteAsync(HomeworkId homeworkId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            HomeworkId = (long)homeworkId
        };

        const string Query =
            $"""
             delete from {HomeworkTable.TableName}
                   where {HomeworkTable.Id} = @{nameof(queryParams.HomeworkId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        await _connectionContext.Connection.ExecuteAsync(command);
    }
}
