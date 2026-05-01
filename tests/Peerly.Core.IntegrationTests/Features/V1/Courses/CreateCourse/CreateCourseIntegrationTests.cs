using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.CreateCourse;

public sealed class CreateCourseIntegrationTests : CreateCourseIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public CreateCourseIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1CreateCourse_TeacherExists_ShouldCreateCourse()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var request = _fixture.Build<V1CreateCourseRequest>()
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);

        // Act
        var response = await CreateCourseClient.V1CreateCourseAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateCourseResponse.ResponseOneofCase.SuccessResponse);
        response.SuccessResponse.CourseId.Should().BePositive();

        var course = await GetCreatedCourseAsync(response.SuccessResponse.CourseId);
        course.Should().BeEquivalentTo((request.Name, request.Description, CourseStatus.Draft.ToString()));

        var isCourseTeacherExists = await IsCourseTeacherExistsAsync(response.SuccessResponse.CourseId, teacherId);
        isCourseTeacherExists.Should().BeTrue();
    }

    [Fact]
    public async Task V1CreateCourse_TeacherNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var request = _fixture.Create<V1CreateCourseRequest>();

        // Act
        var response = await CreateCourseClient.V1CreateCourseAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateCourseResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Преподавателя с таким идентификатором не найдено");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1CreateCourse_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = _fixture.Build<V1CreateCourseRequest>()
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var act = async () => await CreateCourseClient.V1CreateCourseAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }

    [Fact]
    public async Task V1CreateCourse_EmptyName_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1CreateCourseRequest>()
            .With(result => result.Name, string.Empty)
            .Create();

        // Act
        var act = async () => await CreateCourseClient.V1CreateCourseAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Name));
    }

    [Fact]
    public async Task V1CreateCourse_EmptyDescription_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1CreateCourseRequest>()
            .With(result => result.Description, string.Empty)
            .Create();

        // Act
        var act = async () => await CreateCourseClient.V1CreateCourseAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Description));
    }

    private async Task<(string Name, string? Description, string Status)> GetCreatedCourseAsync(long courseId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        var row = await connection.QuerySingleAsync(
            "select name, description, status from courses where id = @courseId",
            new { courseId });
        return (row.name, row.description, row.status);
    }

    private async Task<bool> IsCourseTeacherExistsAsync(long courseId, long teacherId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(
            "select exists(select from course_teachers where course_id = @courseId and teacher_id = @teacherId)",
            new { courseId, teacherId });
    }
}
