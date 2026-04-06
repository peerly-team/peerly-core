using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;
using SubmittedHomeworkStudent = Peerly.Core.Models.Submissions.SubmittedHomeworkStudent;

namespace Peerly.Core.Persistence.Repositories.DistributionReviewers;

internal sealed class DistributionReviewerRepository : IDistributionReviewerRepository
{
    private readonly IConnectionContext _connectionContext;

    public DistributionReviewerRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task BatchAddAsync(IReadOnlyCollection<DistributionReviewerAddItem> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return;
        }

        var queryParams = new
        {
            SubmittedHomeworkIds = items.ToArrayBy(item => (long)item.SubmittedHomeworkId),
            StudentIds = items.ToArrayBy(item => (long)item.StudentId)
        };

        const string Query =
            $"""
             insert into {DistributionReviewerTable.TableName} (
                         {DistributionReviewerTable.SubmittedHomeworkId},
                         {DistributionReviewerTable.StudentId})
             select *
               from unnest(@{nameof(queryParams.SubmittedHomeworkIds)},
                           @{nameof(queryParams.StudentIds)})
             on conflict do nothing;
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        await _connectionContext.Connection.ExecuteAsync(command);
    }

    public async Task<IReadOnlyCollection<AssignedReview>> ListAssignedReviewsAsync(
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
             select dr.{DistributionReviewerTable.SubmittedHomeworkId},
                    h.{HomeworkTable.Id} as {nameof(AssignedReviewDb.HomeworkId)},
                    h.{HomeworkTable.Name} as {nameof(AssignedReviewDb.HomeworkName)},
                    exists(
                        select 1
                          from {SubmittedReviewTable.TableName} sr
                         where sr.{SubmittedReviewTable.SubmittedHomeworkId} = dr.{DistributionReviewerTable.SubmittedHomeworkId}
                           and sr.{SubmittedReviewTable.StudentId} = dr.{DistributionReviewerTable.StudentId}
                    ) as {nameof(AssignedReviewDb.IsReviewed)}
               from {DistributionReviewerTable.TableName} dr
               join {SubmittedHomeworkTable.TableName} sh
                 on sh.{SubmittedHomeworkTable.Id} = dr.{DistributionReviewerTable.SubmittedHomeworkId}
               join {HomeworkTable.TableName} h
                 on h.{HomeworkTable.Id} = sh.{SubmittedHomeworkTable.HomeworkId}
              where dr.{DistributionReviewerTable.StudentId} = @{nameof(queryParams.StudentId)}
                and sh.{SubmittedHomeworkTable.HomeworkId} = @{nameof(queryParams.HomeworkId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var results = await _connectionContext.Connection.QueryAsync<AssignedReviewDb>(command);

        return results.ToArrayBy(db => new AssignedReview
        {
            SubmittedHomeworkId = new SubmittedHomeworkId(db.SubmittedHomeworkId),
            HomeworkId = new HomeworkId(db.HomeworkId),
            HomeworkName = db.HomeworkName,
            IsReviewed = db.IsReviewed
        });
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
             select exists(
                 select 1
                   from {DistributionReviewerTable.TableName}
                  where {DistributionReviewerTable.SubmittedHomeworkId} = @{nameof(queryParams.SubmittedHomeworkId)}
                    and {DistributionReviewerTable.StudentId} = @{nameof(queryParams.StudentId)});
             """;

        var command = new CommandDefinition(
            Query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        return await _connectionContext.Connection.QuerySingleAsync<bool>(command);
    }
}
