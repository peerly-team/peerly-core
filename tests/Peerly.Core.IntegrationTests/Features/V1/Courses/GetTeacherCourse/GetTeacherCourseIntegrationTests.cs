using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;
using ProtoCourseStatus = Peerly.Core.V1.CourseStatus;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.GetTeacherCourse;

public sealed class GetTeacherCourseIntegrationTests : GetTeacherCourseIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public GetTeacherCourseIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1GetTeacherCourse_CourseTeacherExists_ShouldReturnCourseInfo()
    {
        // Arrange
        var courseName = _fixture.Create<string>();
        var courseDescription = _fixture.Create<string>();
        var teacherId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync(courseName, courseDescription);
        var request = _fixture.Build<V1GetTeacherCourseRequest>()
            .With(result => result.CourseId, courseId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        // Act
        var response = await GetTeacherCourseClient.V1GetTeacherCourseAsync(request);

        // Assert
        response.CourseInfo.Id.Should().Be(courseId);
        response.CourseInfo.Name.Should().Be(courseName);
        response.CourseInfo.Description.Should().Be(courseDescription);
        response.CourseInfo.Status.Should().Be(ProtoCourseStatus.Draft);
        response.CourseInfo.StudentCount.Should().Be(0);
        response.CourseInfo.HomeworkCount.Should().Be(0);
    }

    [Fact]
    public async Task V1GetTeacherCourse_GroupTeacherExists_ShouldReturnCourseInfo()
    {
        // Arrange
        var courseName = _fixture.Create<string>();
        var courseDescription = _fixture.Create<string>();
        var courseId = await AddCourseInDbAsync(courseName, courseDescription);
        var otherCourseId = await AddCourseInDbAsync(_fixture.Create<string>(), _fixture.Create<string>());

        var teacherId = _fixture.Create<long>();
        await AddTeacherInDbAsync(teacherId);

        var firstGroupId = await AddGroupInDbAsync(courseId, _fixture.Create<string>());
        var secondGroupId = await AddGroupInDbAsync(courseId, _fixture.Create<string>());
        var otherCourseGroupId = await AddGroupInDbAsync(otherCourseId, _fixture.Create<string>());
        await AddGroupTeacherInDbAsync(firstGroupId, teacherId);
        await AddGroupTeacherInDbAsync(otherCourseGroupId, teacherId);

        var firstStudentId = _fixture.Create<long>();
        var secondStudentId = _fixture.Create<long>();
        var thirdStudentId = _fixture.Create<long>();
        await AddStudentInDbAsync(firstStudentId);
        await AddStudentInDbAsync(secondStudentId);
        await AddStudentInDbAsync(thirdStudentId);
        await AddGroupStudentInDbAsync(firstGroupId, firstStudentId);
        await AddGroupStudentInDbAsync(firstGroupId, secondStudentId);
        await AddGroupStudentInDbAsync(secondGroupId, thirdStudentId);
        await AddGroupStudentInDbAsync(otherCourseGroupId, _fixture.Create<long>());

        await AddHomeworkInDbAsync(courseId, teacherId, _fixture.Create<string>());
        await AddHomeworkInDbAsync(courseId, teacherId, _fixture.Create<string>());
        await AddHomeworkInDbAsync(otherCourseId, teacherId, _fixture.Create<string>());

        var request = _fixture.Build<V1GetTeacherCourseRequest>()
            .With(result => result.CourseId, courseId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var response = await GetTeacherCourseClient.V1GetTeacherCourseAsync(request);

        // Assert
        response.CourseInfo.Id.Should().Be(courseId);
        response.CourseInfo.Name.Should().Be(courseName);
        response.CourseInfo.Description.Should().Be(courseDescription);
        response.CourseInfo.Status.Should().Be(ProtoCourseStatus.Draft);
        response.CourseInfo.StudentCount.Should().Be(3);
        response.CourseInfo.HomeworkCount.Should().Be(2);
    }

    [Fact]
    public async Task V1GetTeacherCourse_CourseTeacherAndGroupTeacherNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var request = _fixture.Build<V1GetTeacherCourseRequest>()
            .With(result => result.CourseId, courseId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);

        // Act
        var act = async () => await GetTeacherCourseClient.V1GetTeacherCourseAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task V1GetTeacherCourse_CourseNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var courseId = _fixture.Create<long>();
        var groupId = await AddGroupInDbAsync(courseId, _fixture.Create<string>());
        var request = _fixture.Build<V1GetTeacherCourseRequest>()
            .With(result => result.CourseId, courseId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);
        await AddGroupTeacherInDbAsync(groupId, teacherId);

        // Act
        var act = async () => await GetTeacherCourseClient.V1GetTeacherCourseAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetTeacherCourse_NotPositiveCourseId_ShouldReturnInvalidArgument(long courseId)
    {
        // Arrange
        var request = _fixture.Build<V1GetTeacherCourseRequest>()
            .With(result => result.CourseId, courseId)
            .Create();

        // Act
        var act = async () => await GetTeacherCourseClient.V1GetTeacherCourseAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.CourseId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetTeacherCourse_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = _fixture.Build<V1GetTeacherCourseRequest>()
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var act = async () => await GetTeacherCourseClient.V1GetTeacherCourseAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }
}
