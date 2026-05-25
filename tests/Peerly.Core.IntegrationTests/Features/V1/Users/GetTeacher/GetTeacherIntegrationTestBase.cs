using System;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.IntegrationTests.Features.V1.Users.GetTeacher.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Users.GetTeacher;

[Collection(IntegrationTestCollection.Name)]
public abstract class GetTeacherIntegrationTestBase : IAsyncLifetime
{
    protected GetTeacherIntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected IntegrationTestFixture Fixture { get; }

    protected GetTeacherGrpcClient GetTeacherClient => Fixture.GetTeacherClient;

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
}
