using System;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.ListSubmittedHomeworkOverview.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.ListSubmittedHomeworkOverview;

public abstract class ListSubmittedHomeworkOverviewIntegrationTestBase : SubmissionIntegrationTestBase
{
    protected ListSubmittedHomeworkOverviewIntegrationTestBase(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    protected ListSubmittedHomeworkOverviewGrpcClient ListSubmittedHomeworkOverviewClient => Fixture.ListSubmittedHomeworkOverviewClient;

    protected async Task AddCourseTeacherInDbAsync(long courseId, long teacherId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into course_teachers (course_id, teacher_id, creation_time)
            values (@courseId, @teacherId, @creationTime)
            on conflict (course_id, teacher_id) do nothing;
            """;

        await connection.ExecuteAsync(
            Query,
            new
            {
                courseId,
                teacherId,
                creationTime = DateTimeOffset.UtcNow
            });
    }

    protected async Task AddGroupTeacherInDbAsync(long groupId, long teacherId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into group_teachers (group_id, teacher_id, creation_time)
            values (@groupId, @teacherId, @creationTime)
            on conflict (group_id, teacher_id) do nothing;
            """;

        await connection.ExecuteAsync(
            Query,
            new
            {
                groupId,
                teacherId,
                creationTime = DateTimeOffset.UtcNow
            });
    }

    protected async Task<long> AddSubmittedReviewInDbAsync(long submittedHomeworkId, long studentId, int mark = 5)
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
                comment = "Review comment",
                creationTime = DateTimeOffset.UtcNow
            });
    }

    protected async Task AddSubmittedHomeworkMarkInDbAsync(
        long submittedHomeworkId,
        int reviewersMark,
        int? teacherMark = null,
        bool hasDiscrepancy = false)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into submitted_homework_marks (
                submitted_homework_id, reviewers_mark, teacher_mark, has_discrepancy, creation_time)
            values (@submittedHomeworkId, @reviewersMark, @teacherMark, @hasDiscrepancy, @creationTime)
            on conflict (submitted_homework_id) do nothing;
            """;

        await connection.ExecuteAsync(
            Query,
            new
            {
                submittedHomeworkId,
                reviewersMark,
                teacherMark,
                hasDiscrepancy,
                creationTime = DateTimeOffset.UtcNow
            });
    }
}
