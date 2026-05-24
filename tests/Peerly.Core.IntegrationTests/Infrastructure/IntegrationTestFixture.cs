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
using Peerly.Core.IntegrationTests.Features.V1.Groups.CreateGroup.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Homeworks.DeleteHomework.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Homeworks.DeleteHomeworkFile.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Homeworks.GetStudentHomework.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Homeworks.GetTeacherHomework.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Homeworks.ListStudentCourseHomeworks.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Homeworks.ListTeacherCourseHomeworks.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Homeworks.SearchStudentHomeworks.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Homeworks.SearchTeacherHomeworks.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.CorrectSubmittedHomeworkMark.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.CreateSubmittedHomework.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.CreateSubmittedHomeworkFile.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.CreateSubmittedReview.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.DeleteSubmittedHomework.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.DeleteSubmittedHomeworkFile.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.DeleteSubmittedReview.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.GetAssignedReview.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.GetSubmittedHomework.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.GetSubmittedReview.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.GetTeacherSubmittedHomework.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.ListAssignedReviews.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.UpdateSubmittedHomework.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.UpdateSubmittedReview.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Users.SearchUsers.Infrastructure;
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
    public CreateGroupGrpcClient CreateGroupClient => ApplicationFactory.CreateGroupClient();
    public DeleteHomeworkGrpcClient DeleteHomeworkClient => ApplicationFactory.DeleteHomeworkClient();
    public DeleteHomeworkFileGrpcClient DeleteHomeworkFileClient => ApplicationFactory.DeleteHomeworkFileClient();
    public GetStudentHomeworkGrpcClient GetStudentHomeworkClient => ApplicationFactory.GetStudentHomeworkClient();
    public GetTeacherHomeworkGrpcClient GetTeacherHomeworkClient => ApplicationFactory.GetTeacherHomeworkClient();
    public ListStudentCourseHomeworksGrpcClient ListStudentCourseHomeworksClient => ApplicationFactory.ListStudentCourseHomeworksClient();
    public ListTeacherCourseHomeworksGrpcClient ListTeacherCourseHomeworksClient => ApplicationFactory.ListTeacherCourseHomeworksClient();
    public SearchStudentHomeworksGrpcClient SearchStudentHomeworksClient => ApplicationFactory.SearchStudentHomeworksClient();
    public SearchTeacherHomeworksGrpcClient SearchTeacherHomeworksClient => ApplicationFactory.SearchTeacherHomeworksClient();
    public CorrectSubmittedHomeworkMarkGrpcClient CorrectSubmittedHomeworkMarkClient => ApplicationFactory.CorrectSubmittedHomeworkMarkClient();
    public CreateSubmittedHomeworkGrpcClient CreateSubmittedHomeworkClient => ApplicationFactory.CreateSubmittedHomeworkClient();
    public CreateSubmittedHomeworkFileGrpcClient CreateSubmittedHomeworkFileClient => ApplicationFactory.CreateSubmittedHomeworkFileClient();
    public CreateSubmittedReviewGrpcClient CreateSubmittedReviewClient => ApplicationFactory.CreateSubmittedReviewClient();
    public GetAssignedReviewGrpcClient GetAssignedReviewClient => ApplicationFactory.GetAssignedReviewClient();
    public GetSubmittedHomeworkGrpcClient GetSubmittedHomeworkClient => ApplicationFactory.GetSubmittedHomeworkClient();
    public GetSubmittedReviewGrpcClient GetSubmittedReviewClient => ApplicationFactory.GetSubmittedReviewClient();
    public GetTeacherSubmittedHomeworkGrpcClient GetTeacherSubmittedHomeworkClient => ApplicationFactory.GetTeacherSubmittedHomeworkClient();
    public ListAssignedReviewsGrpcClient ListAssignedReviewsClient => ApplicationFactory.ListAssignedReviewsClient();
    public DeleteSubmittedHomeworkGrpcClient DeleteSubmittedHomeworkClient => ApplicationFactory.DeleteSubmittedHomeworkClient();
    public DeleteSubmittedHomeworkFileGrpcClient DeleteSubmittedHomeworkFileClient => ApplicationFactory.DeleteSubmittedHomeworkFileClient();
    public DeleteSubmittedReviewGrpcClient DeleteSubmittedReviewClient => ApplicationFactory.DeleteSubmittedReviewClient();
    public UpdateSubmittedHomeworkGrpcClient UpdateSubmittedHomeworkClient => ApplicationFactory.UpdateSubmittedHomeworkClient();
    public UpdateSubmittedReviewGrpcClient UpdateSubmittedReviewClient => ApplicationFactory.UpdateSubmittedReviewClient();
    public SearchUsersGrpcClient SearchUsersClient => ApplicationFactory.SearchUsersClient();

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
