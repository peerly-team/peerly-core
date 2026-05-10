using System;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.GetAssignedReview.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.GetAssignedReview;

public abstract class GetAssignedReviewIntegrationTestBase : SubmissionIntegrationTestBase
{
    protected GetAssignedReviewIntegrationTestBase(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    protected GetAssignedReviewGrpcClient GetAssignedReviewClient => Fixture.GetAssignedReviewClient;

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

    protected async Task<(long Id, string Name, int Size)> AddFileInDbAsync(string name, int size)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into files (storage_id, name, size, creation_time)
            values (@storageId, @name, @size, @creationTime)
            returning id;
            """;

        var fileId = await connection.QuerySingleAsync<long>(
            Query,
            new
            {
                storageId = Guid.NewGuid(),
                name,
                size,
                creationTime = DateTimeOffset.UtcNow
            });

        return (fileId, name, size);
    }

    protected async Task AddSubmittedHomeworkFileInDbAsync(long submittedHomeworkId, long fileId, long? anonymizedFileId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into submitted_homework_files (submitted_homework_id, file_id, anonymized_file_id)
            values (@submittedHomeworkId, @fileId, @anonymizedFileId)
            on conflict (submitted_homework_id, file_id) do nothing;
            """;

        await connection.ExecuteAsync(Query, new { submittedHomeworkId, fileId, anonymizedFileId });
    }
}
