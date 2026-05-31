using System;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.Models.Homeworks;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Rubrics;

[Collection(IntegrationTestCollection.Name)]
public abstract class RubricIntegrationTestBase : IAsyncLifetime
{
    protected RubricIntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected IntegrationTestFixture Fixture { get; }

    public virtual Task InitializeAsync()
    {
        return Fixture.ResetDatabaseAsync();
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected async Task AddTeacherInDbAsync(long teacherId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into teachers (id, email, name, creation_time)
            values (@teacherId, @email, @name, @creationTime)
            on conflict (id) do nothing;
            """;

        await using var command = new NpgsqlCommand(Query, connection);
        command.Parameters.AddWithValue("teacherId", teacherId);
        command.Parameters.AddWithValue("email", $"teacher-{teacherId}@peerly.test");
        command.Parameters.AddWithValue("name", $"Teacher {teacherId}");
        command.Parameters.AddWithValue("creationTime", DateTimeOffset.UtcNow);

        await command.ExecuteNonQueryAsync();
    }

    protected async Task AddStudentInDbAsync(long studentId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into students (id, email, name, creation_time)
            values (@studentId, @email, @name, @creationTime)
            on conflict (id) do nothing;
            """;

        await using var command = new NpgsqlCommand(Query, connection);
        command.Parameters.AddWithValue("studentId", studentId);
        command.Parameters.AddWithValue("email", $"student-{studentId}@peerly.test");
        command.Parameters.AddWithValue("name", $"Student {studentId}");
        command.Parameters.AddWithValue("creationTime", DateTimeOffset.UtcNow);

        await command.ExecuteNonQueryAsync();
    }

    protected async Task<long> AddRubricInDbAsync(long teacherId, string name = "Test Rubric")
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into rubrics (teacher_id, name, creation_time)
            values (@teacherId, @name, @creationTime)
            returning id;
            """;

        return await connection.QuerySingleAsync<long>(
            Query,
            new
            {
                teacherId,
                name,
                creationTime = DateTimeOffset.UtcNow
            });
    }

    protected async Task<long> AddRubricCriterionInDbAsync(
        long rubricId,
        string name = "Criterion",
        int maxScore = 100,
        bool commentRequired = false,
        int position = 1)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into rubric_criteria (rubric_id, name, description, max_score, comment_required, position, creation_time)
            values (@rubricId, @name, @description, @maxScore, @commentRequired, @position, @creationTime)
            returning id;
            """;

        return await connection.QuerySingleAsync<long>(
            Query,
            new
            {
                rubricId,
                name,
                description = "Test criterion",
                maxScore,
                commentRequired,
                position,
                creationTime = DateTimeOffset.UtcNow
            });
    }

    protected async Task<long> AddCourseInDbAsync()
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into courses (name, description, status, creation_time)
            values (@name, @description, @status, @creationTime)
            returning id;
            """;

        return await connection.QuerySingleAsync<long>(
            Query,
            new
            {
                name = $"Course {Guid.NewGuid():N}",
                description = "Test course",
                status = "InProgress",
                creationTime = DateTimeOffset.UtcNow
            });
    }

    protected async Task<long> AddHomeworkInDbAsync(long courseId, long teacherId, HomeworkStatus status, long? rubricId = null)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into homeworks (
                course_id, teacher_id, name, status,
                amount_of_reviewers, description,
                deadline, review_deadline, discrepancy_threshold, rubric_id, creation_time)
            values (
                @courseId, @teacherId, @name, @status,
                @amountOfReviewers, @description,
                @deadline, @reviewDeadline, @discrepancyThreshold, @rubricId, @creationTime)
            returning id;
            """;

        return await connection.QuerySingleAsync<long>(
            Query,
            new
            {
                courseId,
                teacherId,
                name = $"Homework {Guid.NewGuid():N}",
                status = status.ToString(),
                amountOfReviewers = 2,
                description = "Description",
                deadline = DateTimeOffset.UtcNow.AddDays(7),
                reviewDeadline = DateTimeOffset.UtcNow.AddDays(14),
                discrepancyThreshold = 2,
                rubricId,
                creationTime = DateTimeOffset.UtcNow
            });
    }

    protected async Task<(string Name, long TeacherId)> GetRubricAsync(long rubricId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        var row = await connection.QuerySingleAsync(
            "select name, teacher_id from rubrics where id = @rubricId",
            new { rubricId });
        return (row.name, row.teacher_id);
    }

    protected async Task<bool> RubricExistsAsync(long rubricId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.QuerySingleAsync<bool>(
            "select exists(select 1 from rubrics where id = @rubricId)",
            new { rubricId });
    }

    protected async Task<int> GetRubricCriteriaCountAsync(long rubricId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.QuerySingleAsync<int>(
            "select count(*) from rubric_criteria where rubric_id = @rubricId",
            new { rubricId });
    }
}
