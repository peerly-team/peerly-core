using System;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.IntegrationTests.Features.V1.Courses.SearchTeacherCourses.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Xunit;
using CourseStatusModel = Peerly.Core.Models.Courses.CourseStatus;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.SearchTeacherCourses;

[Collection(IntegrationTestCollection.Name)]
public abstract class SearchTeacherCoursesIntegrationTestBase : IAsyncLifetime
{
    protected SearchTeacherCoursesIntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected IntegrationTestFixture Fixture { get; }

    protected SearchTeacherCoursesGrpcClient SearchTeacherCoursesClient => Fixture.SearchTeacherCoursesClient;

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
}
