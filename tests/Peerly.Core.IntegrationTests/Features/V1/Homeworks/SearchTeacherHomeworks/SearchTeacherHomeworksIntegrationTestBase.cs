using System;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.IntegrationTests.Features.V1.Homeworks.SearchTeacherHomeworks.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.Models.Homeworks;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.SearchTeacherHomeworks;

[Collection(IntegrationTestCollection.Name)]
public abstract class SearchTeacherHomeworksIntegrationTestBase : IAsyncLifetime
{
    protected SearchTeacherHomeworksIntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected IntegrationTestFixture Fixture { get; }

    protected SearchTeacherHomeworksGrpcClient SearchTeacherHomeworksClient => Fixture.SearchTeacherHomeworksClient;

    public virtual Task InitializeAsync()
    {
        return Fixture.ResetDatabaseAsync();
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected async Task AddTeacherInDbAsync(long teacherId, string? email = null, string? name = null)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into teachers (id, email, name, creation_time)
            values (@teacherId, @email, @name, @creationTime)
            on conflict (id) do nothing;
            """;

        await connection.ExecuteAsync(
            Query,
            new
            {
                teacherId,
                email = email ?? $"teacher-{teacherId}@peerly.test",
                name = name ?? $"Teacher {teacherId}",
                creationTime = DateTimeOffset.UtcNow
            });
    }

    protected async Task<long> AddCourseInDbAsync(string? name = null, string? description = null)
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
                name = name ?? $"Course {Guid.NewGuid():N}",
                description = description ?? $"Description {Guid.NewGuid():N}",
                status = "Draft",
                creationTime = DateTimeOffset.UtcNow
            });
    }

    protected async Task<long> AddGroupInDbAsync(long courseId, string? name = null)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into groups (course_id, name, creation_time)
            values (@courseId, @name, @creationTime)
            returning id;
            """;

        return await connection.QuerySingleAsync<long>(
            Query,
            new
            {
                courseId,
                name = name ?? $"Group {Guid.NewGuid():N}",
                creationTime = DateTimeOffset.UtcNow
            });
    }

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

    protected async Task<long> AddRubricInDbAsync(long teacherId)
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
                name = $"Rubric {Guid.NewGuid():N}",
                creationTime = DateTimeOffset.UtcNow
            });
    }

    protected async Task<long> AddHomeworkInDbAsync(
        long courseId,
        long teacherId,
        HomeworkStatus status,
        long? groupId = null,
        string? name = null,
        string? description = null,
        DateTimeOffset? deadline = null,
        DateTimeOffset? reviewDeadline = null,
        int amountOfReviewers = 2,
        int discrepancyThreshold = 2,
        long? rubricId = null)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into homeworks (
                course_id, group_id, teacher_id, name, status,
                amount_of_reviewers, description,
                deadline, review_deadline, discrepancy_threshold, rubric_id, creation_time)
            values (
                @courseId, @groupId, @teacherId, @name, @status,
                @amountOfReviewers, @description,
                @deadline, @reviewDeadline, @discrepancyThreshold, @rubricId, @creationTime)
            returning id;
            """;

        return await connection.QuerySingleAsync<long>(
            Query,
            new
            {
                courseId,
                groupId,
                teacherId,
                name = name ?? $"Homework {Guid.NewGuid():N}",
                status = status.ToString(),
                amountOfReviewers,
                description = description ?? "Description",
                deadline = deadline ?? DateTimeOffset.UtcNow.AddDays(7),
                reviewDeadline = reviewDeadline ?? DateTimeOffset.UtcNow.AddDays(14),
                discrepancyThreshold,
                rubricId,
                creationTime = DateTimeOffset.UtcNow
            });
    }
}
