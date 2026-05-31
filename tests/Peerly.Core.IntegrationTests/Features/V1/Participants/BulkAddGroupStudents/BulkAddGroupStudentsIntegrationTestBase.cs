using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.IntegrationTests.Features.V1.Participants.BulkAddGroupStudents.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Participants.BulkAddGroupStudents;

[Collection(IntegrationTestCollection.Name)]
public abstract class BulkAddGroupStudentsIntegrationTestBase : IAsyncLifetime
{
    protected BulkAddGroupStudentsIntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected IntegrationTestFixture Fixture { get; }

    protected BulkAddGroupStudentsGrpcClient BulkAddGroupStudentsClient => Fixture.BulkAddGroupStudentsClient;

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
            values (@id, @email, @name, @creationTime)
            on conflict (id) do nothing;
            """;

        await connection.ExecuteAsync(
            Query,
            new
            {
                id = teacherId,
                email = $"teacher-{teacherId}@peerly.test",
                name = $"Teacher {teacherId}",
                creationTime = DateTimeOffset.UtcNow
            });
    }

    protected async Task AddStudentInDbAsync(long studentId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into students (id, email, name, creation_time)
            values (@id, @email, @name, @creationTime)
            on conflict (id) do nothing;
            """;

        await connection.ExecuteAsync(
            Query,
            new
            {
                id = studentId,
                email = $"student-{studentId}@peerly.test",
                name = $"Student {studentId}",
                creationTime = DateTimeOffset.UtcNow
            });
    }

    protected async Task<long> AddCourseInDbAsync(CourseStatus status = CourseStatus.Draft)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into courses (name, description, status, creation_time)
            values (@name, @description, @status, @creationTime)
            returning id;
            """;

        return await connection.ExecuteScalarAsync<long>(
            Query,
            new
            {
                name = $"Course {Guid.NewGuid():N}",
                description = $"Description {Guid.NewGuid():N}",
                status = status.ToString(),
                creationTime = DateTimeOffset.UtcNow
            });
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

        return await connection.ExecuteScalarAsync<long>(
            Query,
            new
            {
                courseId,
                name = $"Group {Guid.NewGuid():N}",
                creationTime = DateTimeOffset.UtcNow
            });
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

        await connection.ExecuteAsync(Query, new { courseId, teacherId, creationTime = DateTimeOffset.UtcNow });
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

    protected async Task<long[]> ListGroupStudentIdsAsync(long groupId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        var studentIds = await connection.QueryAsync<long>(
            "select student_id from group_students where group_id = @groupId order by student_id",
            new { groupId });

        return studentIds.ToArray();
    }
}
