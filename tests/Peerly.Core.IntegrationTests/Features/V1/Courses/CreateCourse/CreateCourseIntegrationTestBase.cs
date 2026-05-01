using System;
using System.Threading.Tasks;
using Npgsql;
using Peerly.Core.IntegrationTests.Features.V1.Courses.CreateCourse.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.CreateCourse;

[Collection(IntegrationTestCollection.Name)]
public abstract class CreateCourseIntegrationTestBase : IAsyncLifetime
{
    protected CreateCourseIntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected IntegrationTestFixture Fixture { get; }

    protected CreateCourseGrpcClient CreateCourseClient => Fixture.CreateCourseClient;

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
}
