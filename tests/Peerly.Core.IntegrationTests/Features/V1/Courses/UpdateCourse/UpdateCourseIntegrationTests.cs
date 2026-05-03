using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.UpdateCourse;

public sealed class UpdateCourseIntegrationTests : UpdateCourseIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public UpdateCourseIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Theory]
    [InlineData(CourseStatus.Draft)]
    [InlineData(CourseStatus.InProgress)]
    public async Task V1UpdateCourse_CourseTeacherExistsAndCourseCanBeUpdated_ShouldUpdateCourse(CourseStatus initialCourseStatus)
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync(status: initialCourseStatus);
        var request = _fixture.Build<V1UpdateCourseRequest>()
            .With(result => result.CourseId, courseId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        // Act
        var response = await UpdateCourseClient.V1UpdateCourseAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateCourseResponse.ResponseOneofCase.SuccessResponse);

        var course = await GetCourseAsync(courseId);
        course.Should().BeEquivalentTo((request.Name, request.Description, initialCourseStatus.ToString()));
    }

    [Fact]
    public async Task V1UpdateCourse_CourseTeacherNotFound_ShouldBeOtherErrorPermissionDenied()
    {
        // Arrange
        var initialCourseName = _fixture.Create<string>();
        var initialCourseDescription = _fixture.Create<string>();
        var teacherId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync(initialCourseName, initialCourseDescription);
        var request = _fixture.Build<V1UpdateCourseRequest>()
            .With(result => result.CourseId, courseId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);

        // Act
        var response = await UpdateCourseClient.V1UpdateCourseAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateCourseResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.PermissionDenied);
        response.OtherError.Message.Should().BeNull();

        var course = await GetCourseAsync(courseId);
        course.Should().BeEquivalentTo((initialCourseName, initialCourseDescription, CourseStatus.Draft.ToString()));
    }

    [Fact]
    public async Task V1UpdateCourse_CourseNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var courseId = _fixture.Create<long>();
        var request = _fixture.Build<V1UpdateCourseRequest>()
            .With(result => result.CourseId, courseId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        // Act
        var response = await UpdateCourseClient.V1UpdateCourseAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateCourseResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Курс не найден");
    }

    [Theory]
    [InlineData(CourseStatus.Finished)]
    [InlineData(CourseStatus.Canceled)]
    [InlineData(CourseStatus.Deleted)]
    public async Task V1UpdateCourse_CourseCanNotBeUpdated_ShouldBeValidationError(CourseStatus initialCourseStatus)
    {
        // Arrange
        var initialCourseName = _fixture.Create<string>();
        var initialCourseDescription = _fixture.Create<string>();
        var teacherId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync(initialCourseName, initialCourseDescription, initialCourseStatus);
        var request = _fixture.Build<V1UpdateCourseRequest>()
            .With(result => result.CourseId, courseId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        // Act
        var response = await UpdateCourseClient.V1UpdateCourseAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateCourseResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainSingle("Изменить информацию о курсе можно в статусах  \"Черновик\" и \"В процессе\"");

        var course = await GetCourseAsync(courseId);
        course.Should().BeEquivalentTo((initialCourseName, initialCourseDescription, initialCourseStatus.ToString()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1UpdateCourse_NotPositiveCourseId_ShouldReturnInvalidArgument(long courseId)
    {
        // Arrange
        var request = _fixture.Build<V1UpdateCourseRequest>()
            .With(result => result.CourseId, courseId)
            .Create();

        // Act
        var act = async () => await UpdateCourseClient.V1UpdateCourseAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.CourseId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1UpdateCourse_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = _fixture.Build<V1UpdateCourseRequest>()
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var act = async () => await UpdateCourseClient.V1UpdateCourseAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }

    [Fact]
    public async Task V1UpdateCourse_EmptyName_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1UpdateCourseRequest>()
            .With(result => result.Name, string.Empty)
            .Create();

        // Act
        var act = async () => await UpdateCourseClient.V1UpdateCourseAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Name));
    }

    [Fact]
    public async Task V1UpdateCourse_EmptyDescription_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1UpdateCourseRequest>()
            .With(result => result.Description, string.Empty)
            .Create();

        // Act
        var act = async () => await UpdateCourseClient.V1UpdateCourseAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Description));
    }

    private async Task<(string Name, string? Description, string Status)> GetCourseAsync(long courseId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        var row = await connection.QuerySingleAsync(
            "select name, description, status from courses where id = @courseId",
            new { courseId });
        return (row.name, row.description, row.status);
    }
}
