using System;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.CreateSubmittedHomework.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.Models.Homeworks;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.CreateSubmittedHomework;

[Collection(IntegrationTestCollection.Name)]
public abstract class CreateSubmittedHomeworkIntegrationTestBase : IAsyncLifetime
{
    protected CreateSubmittedHomeworkIntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected IntegrationTestFixture Fixture { get; }

    protected CreateSubmittedHomeworkGrpcClient CreateSubmittedHomeworkClient => Fixture.CreateSubmittedHomeworkClient;

    public virtual Task InitializeAsync()
    {
        return Fixture.ResetDatabaseAsync();
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected async Task AddTeacherInDbAsync(long teacherId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into teachers (id, email, name, creation_time)
            values (@teacherId, @email, @name, @creationTime)
            on conflict (id) do nothing;
            """;

        await using var command = new NpgsqlCommand(Query, connection);
        command.Parameters.AddWithValue("teacherId", teacherId);
        command.Parameters.AddWithValue("email", $"teacher-{teacherId}@peerly.test");
        command.Parameters.AddWithValue("name", $"Teacher {teacherId}");
        command.Parameters.AddWithValue("creationTime", DateTimeOffset.UtcNow);

        await command.ExecuteNonQueryAsync();
    }

    protected async Task AddStudentInDbAsync(long studentId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into students (id, email, name, creation_time)
            values (@studentId, @email, @name, @creationTime)
            on conflict (id) do nothing;
            """;

        await using var command = new NpgsqlCommand(Query, connection);
        command.Parameters.AddWithValue("studentId", studentId);
        command.Parameters.AddWithValue("email", $"student-{studentId}@peerly.test");
        command.Parameters.AddWithValue("name", $"Student {studentId}");
        command.Parameters.AddWithValue("creationTime", DateTimeOffset.UtcNow);

        await command.ExecuteNonQueryAsync();
    }

    protected async Task<long> AddCourseInDbAsync()
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into courses (name, description, status, creation_time)
            values (@name, @description, @status, @creationTime)
            returning id;
            """;

        await using var command = new NpgsqlCommand(Query, connection);
        command.Parameters.AddWithValue("name", $"Course {Guid.NewGuid():N}");
        command.Parameters.AddWithValue("description", $"Description {Guid.NewGuid():N}");
        command.Parameters.AddWithValue("status", "InProgress");
        command.Parameters.AddWithValue("creationTime", DateTimeOffset.UtcNow);

        var result = await command.ExecuteScalarAsync();
        return (long)result!;
    }

    protected async Task<long> AddGroupInDbAsync(long courseId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into groups (course_id, name, creation_time)
            values (@courseId, @name, @creationTime)
            returning id;
            """;

        await using var command = new NpgsqlCommand(Query, connection);
        command.Parameters.AddWithValue("courseId", courseId);
        command.Parameters.AddWithValue("name", $"Group {Guid.NewGuid():N}");
        command.Parameters.AddWithValue("creationTime", DateTimeOffset.UtcNow);

        var result = await command.ExecuteScalarAsync();
        return (long)result!;
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

        await connection.ExecuteAsync(Query, new { groupId, studentId, creationTime = DateTimeOffset.UtcNow });
    }

    protected async Task<long> AddHomeworkInDbAsync(
        long courseId,
        long teacherId,
        HomeworkStatus status,
        long? groupId = null,
        DateTimeOffset? deadline = null)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        var query = groupId.HasValue
            ? """
              insert into homeworks (
                  course_id, group_id, teacher_id, name, status,
                  amount_of_reviewers, description, checklist,
                  deadline, review_deadline, discrepancy_threshold, creation_time)
              values (
                  @courseId, @groupId, @teacherId, @name, @status,
                  @amountOfReviewers, @description, @checklist,
                  @deadline, @reviewDeadline, @discrepancyThreshold, @creationTime)
              returning id;
              """
            : """
              insert into homeworks (
                  course_id, teacher_id, name, status,
                  amount_of_reviewers, description, checklist,
                  deadline, review_deadline, discrepancy_threshold, creation_time)
              values (
                  @courseId, @teacherId, @name, @status,
                  @amountOfReviewers, @description, @checklist,
                  @deadline, @reviewDeadline, @discrepancyThreshold, @creationTime)
              returning id;
              """;

        var parameters = new DynamicParameters();
        parameters.Add("courseId", courseId);
        parameters.Add("teacherId", teacherId);
        parameters.Add("name", $"Homework {Guid.NewGuid():N}");
        parameters.Add("status", status.ToString());
        parameters.Add("amountOfReviewers", 2);
        parameters.Add("description", "Description");
        parameters.Add("checklist", "Checklist");
        parameters.Add("deadline", deadline ?? DateTimeOffset.UtcNow.AddDays(7));
        parameters.Add("reviewDeadline", DateTimeOffset.UtcNow.AddDays(14));
        parameters.Add("discrepancyThreshold", 2);
        parameters.Add("creationTime", DateTimeOffset.UtcNow);
        if (groupId.HasValue)
        {
            parameters.Add("groupId", groupId.Value);
        }

        return await connection.QuerySingleAsync<long>(query, parameters);
    }

    protected async Task<long> AddSubmittedHomeworkInDbAsync(long homeworkId, long studentId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into submitted_homeworks (homework_id, student_id, comment, creation_time)
            values (@homeworkId, @studentId, @comment, @creationTime)
            returning id;
            """;

        return await connection.QuerySingleAsync<long>(
            Query,
            new
            {
                homeworkId,
                studentId,
                comment = "Test comment",
                creationTime = DateTimeOffset.UtcNow
            });
    }
}
