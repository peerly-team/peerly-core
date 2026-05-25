using System;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.IntegrationTests.Features.V1.Users.GetStudent.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Users.GetStudent;

[Collection(IntegrationTestCollection.Name)]
public abstract class GetStudentIntegrationTestBase : IAsyncLifetime
{
    protected GetStudentIntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected IntegrationTestFixture Fixture { get; }

    protected GetStudentGrpcClient GetStudentClient => Fixture.GetStudentClient;

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
}
