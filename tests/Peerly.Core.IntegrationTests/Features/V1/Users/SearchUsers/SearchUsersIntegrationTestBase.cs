using System;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.IntegrationTests.Features.V1.Users.SearchUsers.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Users.SearchUsers;

[Collection(IntegrationTestCollection.Name)]
public abstract class SearchUsersIntegrationTestBase : IAsyncLifetime
{
    protected SearchUsersIntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected IntegrationTestFixture Fixture { get; }

    protected SearchUsersGrpcClient SearchUsersClient => Fixture.SearchUsersClient;

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
}
