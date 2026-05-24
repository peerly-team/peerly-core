using System;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.ListAssignedReviews.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.ListAssignedReviews;

public abstract class ListAssignedReviewsIntegrationTestBase : SubmissionIntegrationTestBase
{
    protected ListAssignedReviewsIntegrationTestBase(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    protected ListAssignedReviewsGrpcClient ListAssignedReviewsClient => Fixture.ListAssignedReviewsClient;

    protected async Task AddDistributionReviewerInDbAsync(long submittedHomeworkId, long studentId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into distribution_reviewers (submitted_homework_id, student_id)
            values (@submittedHomeworkId, @studentId)
            on conflict (submitted_homework_id, student_id) do nothing;
            """;

        await connection.ExecuteAsync(Query, new { submittedHomeworkId, studentId });
    }

    protected async Task<long> AddSubmittedReviewInDbAsync(long submittedHomeworkId, long studentId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into submitted_reviews (submitted_homework_id, student_id, mark, comment, creation_time)
            values (@submittedHomeworkId, @studentId, @mark, @comment, @creationTime)
            returning id;
            """;

        return await connection.QuerySingleAsync<long>(
            Query,
            new
            {
                submittedHomeworkId,
                studentId,
                mark = 5,
                comment = "Review comment",
                creationTime = DateTimeOffset.UtcNow
            });
    }

    protected async Task<string> GetHomeworkNameInDbAsync(long homeworkId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            select name
              from homeworks
             where id = @homeworkId;
            """;

        return await connection.QuerySingleAsync<string>(Query, new { homeworkId });
    }
}
