using System;
using System.Threading.Tasks;
using Npgsql;
using Peerly.Core.IntegrationTests.Features.V1.Courses.DeleteCourse.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.DeleteCourse;

[Collection(IntegrationTestCollection.Name)]
public abstract class DeleteCourseIntegrationTestBase : IAsyncLifetime
{
    protected DeleteCourseIntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected IntegrationTestFixture Fixture { get; }

    protected DeleteCourseGrpcClient DeleteCourseClient => Fixture.DeleteCourseClient;

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
            values (@id, @email, @name, @creationTime)
            on conflict (id) do nothing;
            """;

        await using var command = new NpgsqlCommand(Query, connection);
        command.Parameters.AddWithValue("id", teacherId);
        command.Parameters.AddWithValue("email", email ?? $"teacher-{teacherId}@peerly.test");
        command.Parameters.AddWithValue("name", name ?? $"Teacher {teacherId}");
        command.Parameters.AddWithValue("creationTime", DateTimeOffset.UtcNow);

        await command.ExecuteNonQueryAsync();
    }

    protected async Task<long> AddCourseInDbAsync(
        string? name = null,
        string? description = null,
        CourseStatus status = CourseStatus.Draft)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into courses (name, description, status, creation_time)
            values (@name, @description, @status, @creationTime)
            returning id;
            """;

        await using var command = new NpgsqlCommand(Query, connection);
        command.Parameters.AddWithValue("name", name ?? $"Course {Guid.NewGuid():N}");
        command.Parameters.AddWithValue("description", description ?? $"Description {Guid.NewGuid():N}");
        command.Parameters.AddWithValue("status", status.ToString());
        command.Parameters.AddWithValue("creationTime", DateTimeOffset.UtcNow);

        var courseId = await command.ExecuteScalarAsync();
        return (long)courseId!;
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

        await using var command = new NpgsqlCommand(Query, connection);
        command.Parameters.AddWithValue("courseId", courseId);
        command.Parameters.AddWithValue("teacherId", teacherId);
        command.Parameters.AddWithValue("creationTime", DateTimeOffset.UtcNow);

        await command.ExecuteNonQueryAsync();
    }
}
