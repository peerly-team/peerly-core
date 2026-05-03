using System;
using System.Threading.Tasks;
using Npgsql;
using Peerly.Core.IntegrationTests.Features.V1.Courses.CreateCourseFile.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.CreateCourseFile;

[Collection(IntegrationTestCollection.Name)]
public abstract class CreateCourseFileIntegrationTestBase : IAsyncLifetime
{
    protected CreateCourseFileIntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected IntegrationTestFixture Fixture { get; }

    protected CreateCourseFileGrpcClient CreateCourseFileClient => Fixture.CreateCourseFileClient;

    public virtual Task InitializeAsync()
    {
        return Fixture.ResetDatabaseAsync();
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected async Task AddCourseInDbAsync(long courseId, string? name = null, string status = "Draft")
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into courses (id, name, status, creation_time)
            values (@id, @name, @status, @creationTime)
            on conflict (id) do nothing;
            """;

        await using var command = new NpgsqlCommand(Query, connection);
        command.Parameters.AddWithValue("id", courseId);
        command.Parameters.AddWithValue("name", name ?? $"Course {courseId}");
        command.Parameters.AddWithValue("status", status);
        command.Parameters.AddWithValue("creationTime", DateTimeOffset.UtcNow);

        await command.ExecuteNonQueryAsync();
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
