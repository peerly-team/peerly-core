using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Persistence.Common;
using Peerly.Core.Persistence.Repositories.SubmittedReviews.Models;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.SubmittedReviews;

internal sealed class SubmittedReviewRepository : ISubmittedReviewRepository
{
    private readonly IConnectionContext _connectionContext;

    public SubmittedReviewRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<SubmittedReviewId> AddAsync(SubmittedReviewAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            SubmittedHomeworkId = (long)item.SubmittedHomeworkId,
            StudentId = (long)item.StudentId,
            item.Mark,
            item.Comment,
            item.CreationTime
        };

        const string Query =
            $"""
             insert into {SubmittedReviewTable.TableName} (
                         {SubmittedReviewTable.SubmittedHomeworkId},
                         {SubmittedReviewTable.StudentId},
                         {SubmittedReviewTable.Mark},
                         {SubmittedReviewTable.Comment},
                         {SubmittedReviewTable.CreationTime})
                  values (
                         @{nameof(queryParams.SubmittedHomeworkId)},
                         @{nameof(queryParams.StudentId)},
                         @{nameof(queryParams.Mark)},
                         @{nameof(queryParams.Comment)},
                         @{nameof(queryParams.CreationTime)})
               returning {SubmittedReviewTable.Id};
             """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var submittedReviewId = await _connectionContext.Connection.QuerySingleAsync<long>(command);

        return new SubmittedReviewId(submittedReviewId);
    }

    public async Task<bool> UpdateAsync(
        SubmittedReviewId submittedReviewId,
        Action<IUpdateBuilder<SubmittedReviewUpdateItem>> configureUpdate,
        CancellationToken cancellationToken)
    {
        var builder = new UpdateBuilder<SubmittedReviewUpdateItem>();
        configureUpdate(builder);

        var configuration = builder.Build();
        var queryParams = configuration.GetQueryParams();
        queryParams.Add($"@{nameof(submittedReviewId)}", (long)submittedReviewId);

        var query =
            $"""
             update {SubmittedReviewTable.TableName} as new
                set {SubmittedReviewTable.UpdateTime} = now(),
                    {SubmittedReviewTable.Mark} = case
                    when {configuration.GetFlagParamName(item => item.Mark)}
                    then {configuration.GetParamName(item => item.Mark)}
                    else {SubmittedReviewTable.Mark}
                    end,
                    {SubmittedReviewTable.Comment} = case
                    when {configuration.GetFlagParamName(item => item.Comment)}
                    then {configuration.GetParamName(item => item.Comment)}
                    else {SubmittedReviewTable.Comment}
                    end
              from (select {SubmittedReviewTable.Id}
                      from {SubmittedReviewTable.TableName}
                     where {SubmittedReviewTable.Id} = @{nameof(submittedReviewId)}
                       for update) as old
             WHERE new.{SubmittedReviewTable.Id} = old.{SubmittedReviewTable.Id};
             """;

        var command = new CommandDefinition(
            query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var affectedRows = await _connectionContext.Connection.ExecuteAsync(command);

        return affectedRows == 1;
    }

    public async Task<bool> ExistsAsync(SubmittedHomeworkStudent submittedHomeworkStudent, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            SubmittedHomeworkId = (long)submittedHomeworkStudent.SubmittedHomeworkId,
            StudentId = (long)submittedHomeworkStudent.StudentId
        };

        const string Query =
            $"""
             select exists(select 1
                             from {SubmittedReviewTable.TableName}
                            where {SubmittedReviewTable.SubmittedHomeworkId} = @{nameof(queryParams.SubmittedHomeworkId)}
                              and {SubmittedReviewTable.StudentId} = @{nameof(queryParams.StudentId)});
             """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        return await _connectionContext.Connection.QuerySingleAsync<bool>(command);
    }

    public async Task<SubmittedReviewId?> GetSubmittedReviewIdAsync(
        SubmittedHomeworkStudent submittedHomeworkStudent,
        CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            SubmittedHomeworkId = (long)submittedHomeworkStudent.SubmittedHomeworkId,
            StudentId = (long)submittedHomeworkStudent.StudentId
        };

        const string Query =
            $"""
             select {SubmittedReviewTable.Id}
               from {SubmittedReviewTable.TableName}
              where {SubmittedReviewTable.SubmittedHomeworkId} = @{nameof(queryParams.SubmittedHomeworkId)}
                and {SubmittedReviewTable.StudentId} = @{nameof(queryParams.StudentId)};
             """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var id = await _connectionContext.Connection.QuerySingleOrDefaultAsync<long?>(command);

        return id is not null ? new SubmittedReviewId(id.Value) : null;
    }

    public async Task<IReadOnlyCollection<SubmittedReview>> ListBySubmittedHomeworkAsync(
        SubmittedHomeworkId submittedHomeworkId,
        CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            SubmittedHomeworkId = (long)submittedHomeworkId
        };

        const string Query =
            $"""
             select {SubmittedReviewTable.Id},
                    {SubmittedReviewTable.SubmittedHomeworkId},
                    {SubmittedReviewTable.StudentId},
                    {SubmittedReviewTable.Mark},
                    {SubmittedReviewTable.Comment},
                    {SubmittedReviewTable.CreationTime}
               from {SubmittedReviewTable.TableName}
              where {SubmittedReviewTable.SubmittedHomeworkId} = @{nameof(queryParams.SubmittedHomeworkId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var dbs = await _connectionContext.Connection.QueryAsync<SubmittedReviewDb>(command);

        return dbs.ToArrayBy(db => db.ToSubmittedReview());
    }

    public async Task<IReadOnlyCollection<SubmittedHomeworkId>> ListReviewedByAsync(
        StudentId studentId,
        HomeworkId homeworkId,
        CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            StudentId = (long)studentId,
            HomeworkId = (long)homeworkId
        };

        const string Query =
            $"""
             select sr.{SubmittedReviewTable.SubmittedHomeworkId}
               from {SubmittedReviewTable.TableName} sr
               join {SubmittedHomeworkTable.TableName} sh on sh.{SubmittedHomeworkTable.Id} = sr.{SubmittedReviewTable.SubmittedHomeworkId}
              where sr.{SubmittedReviewTable.StudentId} = @{nameof(queryParams.StudentId)}
                and sh.{SubmittedHomeworkTable.HomeworkId} = @{nameof(queryParams.HomeworkId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var ids = await _connectionContext.Connection.QueryAsync<long>(command);

        return ids.ToArrayBy(id => new SubmittedHomeworkId(id));
    }

    public async Task<IReadOnlyCollection<SubmittedHomeworkReviewerMark>> ListSubmittedReviewMarksAsync(
        HomeworkId homeworkId,
        CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            HomeworkId = (long)homeworkId
        };

        const string Query =
            $"""
             select sr.{SubmittedReviewTable.SubmittedHomeworkId},
                    sr.{SubmittedReviewTable.Mark}
               from {SubmittedReviewTable.TableName} sr
               join {SubmittedHomeworkTable.TableName} sh on sh.{SubmittedHomeworkTable.Id} = sr.{SubmittedReviewTable.SubmittedHomeworkId}
              where sh.{SubmittedHomeworkTable.HomeworkId} = @{nameof(queryParams.HomeworkId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var results = await _connectionContext.Connection.QueryAsync<SubmittedHomeworkReviewerMarkDb>(command);

        return results.ToArrayBy(db => db.ToSubmittedHomeworkReviewerMark());
    }
}
