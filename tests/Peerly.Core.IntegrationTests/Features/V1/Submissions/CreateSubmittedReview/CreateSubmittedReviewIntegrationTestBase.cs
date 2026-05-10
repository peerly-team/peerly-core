using System;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.CreateSubmittedReview.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.CreateSubmittedReview;

public abstract class CreateSubmittedReviewIntegrationTestBase : SubmissionIntegrationTestBase
{
    protected CreateSubmittedReviewIntegrationTestBase(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    protected CreateSubmittedReviewGrpcClient CreateSubmittedReviewClient => Fixture.CreateSubmittedReviewClient;

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
}
