using System;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Peerly.Core.IntegrationTests.Features.V1.Homeworks.DeleteHomeworkFile.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.Models.Homeworks;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.DeleteHomeworkFile;

[Collection(IntegrationTestCollection.Name)]
public abstract class DeleteHomeworkFileIntegrationTestBase : IAsyncLifetime
{
    protected DeleteHomeworkFileIntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected IntegrationTestFixture Fixture { get; }

    protected DeleteHomeworkFileGrpcClient DeleteHomeworkFileClient => Fixture.DeleteHomeworkFileClient;

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
                status = "Draft",
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

    protected async Task AddHomeworkFileInDbAsync(long homeworkId, long fileId, long teacherId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into homework_files (homework_id, file_id, teacher_id)
            values (@homeworkId, @fileId, @teacherId)
            on conflict (homework_id, file_id) do nothing;
            """;

        await connection.ExecuteAsync(Query, new { homeworkId, fileId, teacherId });
    }
}
