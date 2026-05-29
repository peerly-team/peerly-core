using System;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.IntegrationTests.Features.V1.Homeworks.GetTeacherHomework.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.Models.Homeworks;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.GetTeacherHomework;

[Collection(IntegrationTestCollection.Name)]
public abstract class GetTeacherHomeworkIntegrationTestBase : IAsyncLifetime
{
    protected GetTeacherHomeworkIntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected IntegrationTestFixture Fixture { get; }

    protected GetTeacherHomeworkGrpcClient GetTeacherHomeworkClient => Fixture.GetTeacherHomeworkClient;

    public virtual Task InitializeAsync()
    {
        return Fixture.ResetDatabaseAsync();
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected async Task AddStudentInDbAsync(long studentId, string? email = null, string? name = null)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into students (id, email, name, creation_time)
            values (@studentId, @email, @name, @creationTime)
            on conflict (id) do nothing;
            """;

        await connection.ExecuteAsync(
            Query,
            new
            {
                studentId,
                email = email ?? $"student-{studentId}@peerly.test",
                name = name ?? $"Student {studentId}",
                creationTime = DateTimeOffset.UtcNow
            });
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

    protected async Task<long> AddSubmittedHomeworkInDbAsync(long homeworkId, long studentId, string? comment = null)
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
                comment = comment ?? "Submitted",
                creationTime = DateTimeOffset.UtcNow
            });
    }

    protected async Task<(long Id, string Name, int Size)> AddHomeworkFileInDbAsync(
        long homeworkId,
        long teacherId,
        string name,
        int size)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string AddFileQuery =
            """
            insert into files (storage_id, name, size, creation_time)
            values (@storageId, @name, @size, @creationTime)
            returning id;
            """;

        var fileId = await connection.QuerySingleAsync<long>(
            AddFileQuery,
            new
            {
                storageId = Guid.NewGuid(),
                name,
                size,
                creationTime = DateTimeOffset.UtcNow
            });

        const string AddHomeworkFileQuery =
            """
            insert into homework_files (homework_id, file_id, teacher_id)
            values (@homeworkId, @fileId, @teacherId);
            """;

        await connection.ExecuteAsync(
            AddHomeworkFileQuery,
            new
            {
                homeworkId,
                fileId,
                teacherId
            });

        return (fileId, name, size);
    }
}
