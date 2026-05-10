using System;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.Models.Homeworks;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions;

[Collection(IntegrationTestCollection.Name)]
public abstract class SubmissionIntegrationTestBase : IAsyncLifetime
{
    protected SubmissionIntegrationTestBase(IntegrationTestFixture fixture)
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
                description = $"Description {Guid.NewGuid():N}",
                status = "InProgress",
                creationTime = DateTimeOffset.UtcNow
            });
    }

    protected async Task<long> AddHomeworkInDbAsync(
        long courseId,
        long teacherId,
        HomeworkStatus status,
        DateTimeOffset? reviewDeadline = null)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into homeworks (
                course_id, teacher_id, name, status,
                amount_of_reviewers, description, checklist,
                deadline, review_deadline, discrepancy_threshold, creation_time)
            values (
                @courseId, @teacherId, @name, @status,
                @amountOfReviewers, @description, @checklist,
                @deadline, @reviewDeadline, @discrepancyThreshold, @creationTime)
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
                checklist = "Checklist",
                deadline = DateTimeOffset.UtcNow.AddDays(7),
                reviewDeadline = reviewDeadline ?? DateTimeOffset.UtcNow.AddDays(14),
                discrepancyThreshold = 2,
                creationTime = DateTimeOffset.UtcNow
            });
    }

    protected async Task<long> AddSubmittedHomeworkInDbAsync(long homeworkId, long studentId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into submitted_homeworks (homework_id, student_id, comment, creation_time)
            values (@homeworkId, @studentId, @comment, @creationTime)
            returning id;
            """;

        return await connection.QuerySingleAsync<long>(
            Query,
            new
            {
                homeworkId,
                studentId,
                comment = "Test comment",
                creationTime = DateTimeOffset.UtcNow
            });
    }

    protected async Task AddSubmittedHomeworkFileInDbAsync(long submittedHomeworkId, long fileId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into submitted_homework_files (submitted_homework_id, file_id)
            values (@submittedHomeworkId, @fileId)
            on conflict (submitted_homework_id, file_id) do nothing;
            """;

        await connection.ExecuteAsync(Query, new { submittedHomeworkId, fileId });
    }
}
