using System;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.IntegrationTests.Features.V1.Courses.GetTeacherCourse.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Xunit;
using CourseStatusModel = Peerly.Core.Models.Courses.CourseStatus;
using HomeworkStatusModel = Peerly.Core.Models.Homeworks.HomeworkStatus;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.GetTeacherCourse;

[Collection(IntegrationTestCollection.Name)]
public abstract class GetTeacherCourseIntegrationTestBase : IAsyncLifetime
{
    protected GetTeacherCourseIntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected IntegrationTestFixture Fixture { get; }

    protected GetTeacherCourseGrpcClient GetTeacherCourseClient => Fixture.GetTeacherCourseClient;

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

    protected async Task<long> AddCourseInDbAsync(
        string name,
        string description,
        CourseStatusModel status = CourseStatusModel.Draft)
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
                name,
                description,
                status = status.ToString(),
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

    protected async Task<(long Id, string Name, int Size)> AddCourseFileInDbAsync(
        long courseId,
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

        const string AddCourseFileQuery =
            """
            insert into course_files (course_id, file_id, teacher_id)
            values (@courseId, @fileId, @teacherId);
            """;

        await connection.ExecuteAsync(
            AddCourseFileQuery,
            new
            {
                courseId,
                fileId,
                teacherId
            });

        return (fileId, name, size);
    }

    protected async Task<long> AddGroupInDbAsync(long courseId, string name)
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
                name,
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

    protected async Task AddHomeworkInDbAsync(long courseId, long teacherId, string name, long? rubricId = null)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into homeworks (
                course_id,
                teacher_id,
                name,
                status,
                amount_of_reviewers,
                description,
                deadline,
                review_deadline,
                discrepancy_threshold,
                rubric_id,
                creation_time)
            values (
                @courseId,
                @teacherId,
                @name,
                @status,
                @amountOfReviewers,
                @description,
                @deadline,
                @reviewDeadline,
                @discrepancyThreshold,
                @rubricId,
                @creationTime);
            """;

        await connection.ExecuteAsync(
            Query,
            new
            {
                courseId,
                teacherId,
                name,
                status = HomeworkStatusModel.Draft.ToString(),
                amountOfReviewers = 1,
                description = $"Description {name}",
                deadline = DateTimeOffset.UtcNow.AddDays(7),
                reviewDeadline = DateTimeOffset.UtcNow.AddDays(14),
                discrepancyThreshold = 1,
                rubricId,
                creationTime = DateTimeOffset.UtcNow
            });
    }
}
