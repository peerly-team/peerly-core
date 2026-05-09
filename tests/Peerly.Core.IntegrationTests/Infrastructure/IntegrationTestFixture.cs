using System;
using System.Threading.Tasks;
using Npgsql;
using Peerly.Core.IntegrationTests.Features.V1.Courses.CreateCourse.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Courses.CreateCourseFile.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Courses.DeleteCourse.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Courses.GetStudentCourse.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Courses.GetTeacherCourse.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Courses.PublishCourse.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Courses.SearchStudentCourses.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Courses.SearchTeacherCourses.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Courses.UpdateCourse.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.CorrectSubmittedHomeworkMark.Infrastructure;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;

namespace Peerly.Core.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private const string DatabaseName = "peerly-core-tests";
    private const string DatabaseUsername = "peerly-core-user";
    private const string DatabasePassword = "pwd";

    private Respawner _respawner = null!;
    private WebApplicationFactory? _applicationFactory;
    private NpgsqlDataSource? _dataSource;
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:latest")
        .WithDatabase(DatabaseName)
        .WithUsername(DatabaseUsername)
        .WithPassword(DatabasePassword)
        .Build();

    public WebApplicationFactory ApplicationFactory => _applicationFactory ?? throw new InvalidOperationException("Integration fixture is not initialized.");
    public NpgsqlDataSource DataSource => _dataSource ?? throw new InvalidOperationException("Integration fixture is not initialized.");

    public CreateCourseGrpcClient CreateCourseClient => ApplicationFactory.CreateCourseClient();
    public CreateCourseFileGrpcClient CreateCourseFileClient => ApplicationFactory.CreateCourseFileClient();
    public GetStudentCourseGrpcClient GetStudentCourseClient => ApplicationFactory.GetStudentCourseClient();
    public GetTeacherCourseGrpcClient GetTeacherCourseClient => ApplicationFactory.GetTeacherCourseClient();
    public SearchStudentCoursesGrpcClient SearchStudentCoursesClient => ApplicationFactory.SearchStudentCoursesClient();
    public SearchTeacherCoursesGrpcClient SearchTeacherCoursesClient => ApplicationFactory.SearchTeacherCoursesClient();
    public UpdateCourseGrpcClient UpdateCourseClient => ApplicationFactory.UpdateCourseClient();
    public PublishCourseGrpcClient PublishCourseClient => ApplicationFactory.PublishCourseClient();
    public DeleteCourseGrpcClient DeleteCourseClient => ApplicationFactory.DeleteCourseClient();
    public CorrectSubmittedHomeworkMarkGrpcClient CorrectSubmittedHomeworkMarkClient => ApplicationFactory.CorrectSubmittedHomeworkMarkClient();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();

        _dataSource = NpgsqlDataSource.Create(_database.GetConnectionString());
        await PostgresMigrationRunner.ApplyAsync(_dataSource);

        await using var connection = await _dataSource.OpenConnectionAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });

        _applicationFactory = new WebApplicationFactory(
            _database.Hostname,
            _database.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort),
            DatabaseName,
            DatabaseUsername,
            DatabasePassword);
        await _applicationFactory.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_applicationFactory is not null)
        {
            await _applicationFactory.DisposeAsync();
        }

        if (_dataSource is not null)
        {
            await DataSource.DisposeAsync();
        }

        await _database.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = await DataSource.OpenConnectionAsync();
        await _respawner.ResetAsync(connection);
    }
}
