using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;
using ProtoCourseStatus = Peerly.Core.V1.CourseStatus;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.GetStudentCourse;

public sealed class GetStudentCourseIntegrationTests : GetStudentCourseIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public GetStudentCourseIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1GetStudentCourse_CourseStudentExists_ShouldReturnCourseInfo()
    {
        // Arrange
        var courseName = _fixture.Create<string>();
        var courseDescription = _fixture.Create<string>();
        var courseId = await AddCourseInDbAsync(courseName, courseDescription);
        var otherCourseId = await AddCourseInDbAsync(_fixture.Create<string>(), _fixture.Create<string>());

        var studentId = _fixture.Create<long>();
        var secondStudentId = _fixture.Create<long>();
        var thirdStudentId = _fixture.Create<long>();
        await AddStudentInDbAsync(studentId);
        await AddStudentInDbAsync(secondStudentId);
        await AddStudentInDbAsync(thirdStudentId);

        var firstGroupId = await AddGroupInDbAsync(courseId, _fixture.Create<string>());
        var secondGroupId = await AddGroupInDbAsync(courseId, _fixture.Create<string>());
        var otherCourseGroupId = await AddGroupInDbAsync(otherCourseId, _fixture.Create<string>());
        await AddGroupStudentInDbAsync(firstGroupId, studentId);
        await AddGroupStudentInDbAsync(firstGroupId, secondStudentId);
        await AddGroupStudentInDbAsync(secondGroupId, thirdStudentId);
        await AddGroupStudentInDbAsync(otherCourseGroupId, _fixture.Create<long>());

        var teacherId = _fixture.Create<long>();
        await AddHomeworkInDbAsync(courseId, teacherId, _fixture.Create<string>());
        await AddHomeworkInDbAsync(courseId, teacherId, _fixture.Create<string>());
        await AddHomeworkInDbAsync(otherCourseId, teacherId, _fixture.Create<string>());
        var firstFile = await AddCourseFileInDbAsync(courseId, teacherId, _fixture.Create<string>(), 1024);
        var secondFile = await AddCourseFileInDbAsync(courseId, teacherId, _fixture.Create<string>(), 2048);
        await AddCourseFileInDbAsync(otherCourseId, teacherId, _fixture.Create<string>(), 4096);

        var request = _fixture.Build<V1GetStudentCourseRequest>()
            .With(result => result.CourseId, courseId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await GetStudentCourseClient.V1GetStudentCourseAsync(request);

        // Assert
        response.CourseInfo.Id.Should().Be(courseId);
        response.CourseInfo.Name.Should().Be(courseName);
        response.CourseInfo.Description.Should().Be(courseDescription);
        response.CourseInfo.Status.Should().Be(ProtoCourseStatus.Draft);
        response.StudentCount.Should().Be(3);
        response.HomeworkCount.Should().Be(2);
        response.Files
            .Select(file => new { file.Id, file.Name, file.Size })
            .Should()
            .BeEquivalentTo(
            [
                new { firstFile.Id, firstFile.Name, firstFile.Size },
                new { secondFile.Id, secondFile.Name, secondFile.Size }
            ]);
    }

    [Fact]
    public async Task V1GetStudentCourse_CourseNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var request = _fixture.Create<V1GetStudentCourseRequest>();

        // Act
        var act = async () => await GetStudentCourseClient.V1GetStudentCourseAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task V1GetStudentCourse_CourseStudentNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var courseId = await AddCourseInDbAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var request = _fixture.Build<V1GetStudentCourseRequest>()
            .With(result => result.CourseId, courseId)
            .Create();

        // Act
        var act = async () => await GetStudentCourseClient.V1GetStudentCourseAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetStudentCourse_NotPositiveCourseId_ShouldReturnInvalidArgument(long courseId)
    {
        // Arrange
        var request = _fixture.Build<V1GetStudentCourseRequest>()
            .With(result => result.CourseId, courseId)
            .Create();

        // Act
        var act = async () => await GetStudentCourseClient.V1GetStudentCourseAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.CourseId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetStudentCourse_NotPositiveStudentId_ShouldReturnInvalidArgument(long studentId)
    {
        // Arrange
        var request = _fixture.Build<V1GetStudentCourseRequest>()
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var act = async () => await GetStudentCourseClient.V1GetStudentCourseAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StudentId));
    }
}
