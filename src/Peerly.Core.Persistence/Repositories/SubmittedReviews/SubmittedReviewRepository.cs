using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;
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

    public async Task<IReadOnlyCollection<SubmittedReview>> ListBySubmittedHomeworkIdAsync(
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
        var results = await _connectionContext.Connection.QueryAsync<SubmittedReviewDb>(command);

        return results.ToArrayBy(db => new SubmittedReview
        {
            Id = new SubmittedReviewId(db.Id),
            SubmittedHomeworkId = new SubmittedHomeworkId(db.SubmittedHomeworkId),
            StudentId = new StudentId(db.StudentId),
            Mark = db.Mark,
            Comment = db.Comment,
            CreationTime = db.CreationTime
        });
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
