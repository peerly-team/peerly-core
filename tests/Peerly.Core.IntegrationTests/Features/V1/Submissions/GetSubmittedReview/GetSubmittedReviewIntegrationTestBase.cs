using System;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.GetSubmittedReview.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.GetSubmittedReview;

public abstract class GetSubmittedReviewIntegrationTestBase : SubmissionIntegrationTestBase
{
    protected GetSubmittedReviewIntegrationTestBase(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    protected GetSubmittedReviewGrpcClient GetSubmittedReviewClient => Fixture.GetSubmittedReviewClient;

    protected async Task<long> AddSubmittedReviewInDbAsync(
        long submittedHomeworkId,
        long studentId,
        int mark = 5,
        string comment = "Review comment")
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
                mark,
                comment,
                creationTime = DateTimeOffset.UtcNow
            });
    }
}
