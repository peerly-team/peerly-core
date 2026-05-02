using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.DeleteCourse;

public sealed class DeleteCourseIntegrationTest : DeleteCourseIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public DeleteCourseIntegrationTest(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1DeleteCourse_CourseTeacherExistsAndCourseInDraftStatus_ShouldDeleteCourse()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync(status: CourseStatus.Draft);
        var request = _fixture.Build<V1DeleteCourseRequest>()
            .With(result => result.CourseId, courseId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        // Act
        var response = await DeleteCourseClient.V1DeleteCourseAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteCourseResponse.ResponseOneofCase.SuccessResponse);

        var courseStatus = await GetCourseStatusAsync(courseId);
        courseStatus.Should().Be(CourseStatus.Deleted.ToString());
    }

    [Fact]
    public async Task V1DeleteCourse_CourseTeacherNotFound_ShouldBeOtherErrorPermissionDenied()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync(status: CourseStatus.Draft);
        var request = _fixture.Build<V1DeleteCourseRequest>()
            .With(result => result.CourseId, courseId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);

        // Act
        var response = await DeleteCourseClient.V1DeleteCourseAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteCourseResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.PermissionDenied);
        response.OtherError.Message.Should().BeNull();

        var courseStatus = await GetCourseStatusAsync(courseId);
        courseStatus.Should().Be(CourseStatus.Draft.ToString());
    }

    [Fact]
    public async Task V1DeleteCourse_CourseNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var courseId = _fixture.Create<long>();
        var request = _fixture.Build<V1DeleteCourseRequest>()
            .With(result => result.CourseId, courseId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        // Act
        var response = await DeleteCourseClient.V1DeleteCourseAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteCourseResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Курс не найден");
    }

    [Theory]
    [InlineData(CourseStatus.InProgress)]
    [InlineData(CourseStatus.Finished)]
    [InlineData(CourseStatus.Canceled)]
    [InlineData(CourseStatus.Deleted)]
    public async Task V1DeleteCourse_CourseNotInDraftStatus_ShouldBeValidationError(CourseStatus initialCourseStatus)
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync(status: initialCourseStatus);
        var request = _fixture.Build<V1DeleteCourseRequest>()
            .With(result => result.CourseId, courseId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        // Act
        var response = await DeleteCourseClient.V1DeleteCourseAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteCourseResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainSingle("Удалить курс можно только в статусе \"Черновик\"");

        var finalCourseStatus = await GetCourseStatusAsync(courseId);
        finalCourseStatus.Should().Be(initialCourseStatus.ToString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1DeleteCourse_NotPositiveCourseId_ShouldReturnInvalidArgument(long courseId)
    {
        // Arrange
        var request = _fixture.Build<V1DeleteCourseRequest>()
            .With(result => result.CourseId, courseId)
            .Create();

        // Act
        var act = async () => await DeleteCourseClient.V1DeleteCourseAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.CourseId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1DeleteCourse_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = _fixture.Build<V1DeleteCourseRequest>()
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var act = async () => await DeleteCourseClient.V1DeleteCourseAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }

    private async Task<string?> GetCourseStatusAsync(long courseId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<string>(
            "select status from courses where id = @courseId",
            new { courseId });
    }
}
