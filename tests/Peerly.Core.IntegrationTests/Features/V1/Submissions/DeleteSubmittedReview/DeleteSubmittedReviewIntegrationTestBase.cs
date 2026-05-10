using System;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.DeleteSubmittedReview.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.DeleteSubmittedReview;

public abstract class DeleteSubmittedReviewIntegrationTestBase : SubmissionIntegrationTestBase
{
    protected DeleteSubmittedReviewIntegrationTestBase(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    protected DeleteSubmittedReviewGrpcClient DeleteSubmittedReviewClient => Fixture.DeleteSubmittedReviewClient;

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
