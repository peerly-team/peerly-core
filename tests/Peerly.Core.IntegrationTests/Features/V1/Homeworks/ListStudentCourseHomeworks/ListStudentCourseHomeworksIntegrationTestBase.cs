using System;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.IntegrationTests.Features.V1.Homeworks.ListStudentCourseHomeworks.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.Models.Homeworks;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.ListStudentCourseHomeworks;

[Collection(IntegrationTestCollection.Name)]
public abstract class ListStudentCourseHomeworksIntegrationTestBase : IAsyncLifetime
{
    protected ListStudentCourseHomeworksIntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected IntegrationTestFixture Fixture { get; }

    protected ListStudentCourseHomeworksGrpcClient ListStudentCourseHomeworksClient => Fixture.ListStudentCourseHomeworksClient;

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

    protected async Task AddGroupStudentInDbAsync(long groupId, long studentId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into group_students (group_id, student_id, creation_time)
            values (@groupId, @studentId, @creationTime)
            on conflict (group_id, student_id) do nothing;
            """;

        await connection.ExecuteAsync(
            Query,
            new
            {
                groupId,
                studentId,
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
        string? checklist = null,
        DateTimeOffset? deadline = null,
        DateTimeOffset? reviewDeadline = null,
        int amountOfReviewers = 2)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into homeworks (
                course_id, group_id, teacher_id, name, status,
                amount_of_reviewers, description, checklist,
                deadline, review_deadline, discrepancy_threshold, creation_time)
            values (
                @courseId, @groupId, @teacherId, @name, @status,
                @amountOfReviewers, @description, @checklist,
                @deadline, @reviewDeadline, @discrepancyThreshold, @creationTime)
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
                checklist = checklist ?? "Checklist",
                deadline = deadline ?? DateTimeOffset.UtcNow.AddDays(7),
                reviewDeadline = reviewDeadline ?? DateTimeOffset.UtcNow.AddDays(14),
                discrepancyThreshold = 2,
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
}
