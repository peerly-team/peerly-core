using System;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.IntegrationTests.Features.V1.Courses.SearchStudentCourses.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Xunit;
using CourseStatusModel = Peerly.Core.Models.Courses.CourseStatus;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.SearchStudentCourses;

[Collection(IntegrationTestCollection.Name)]
public abstract class SearchStudentCoursesIntegrationTestBase : IAsyncLifetime
{
    protected SearchStudentCoursesIntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected IntegrationTestFixture Fixture { get; }

    protected SearchStudentCoursesGrpcClient SearchStudentCoursesClient => Fixture.SearchStudentCoursesClient;

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
}
