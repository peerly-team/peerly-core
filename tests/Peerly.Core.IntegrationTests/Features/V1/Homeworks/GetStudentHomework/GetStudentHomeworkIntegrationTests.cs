using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;
using HomeworkStatusModel = Peerly.Core.Models.Homeworks.HomeworkStatus;
using ProtoHomeworkStatus = Peerly.Core.V1.HomeworkStatus;

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.GetStudentHomework;

public sealed class GetStudentHomeworkIntegrationTests : GetStudentHomeworkIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public GetStudentHomeworkIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1GetStudentHomework_CourseStudentExists_ShouldReturnStudentHomework()
    {
        // Arrange
        var studentId = _fixture.Create<long>();
        var teacherId = _fixture.Create<long>();
        await AddStudentInDbAsync(studentId);
        await AddTeacherInDbAsync(teacherId);

        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);
        await AddGroupStudentInDbAsync(groupId, studentId);

        var rubricId = await AddRubricInDbAsync(teacherId);
        var homeworkName = _fixture.Create<string>();
        var homeworkDescription = _fixture.Create<string>();
        var deadline = DateTimeOffset.UtcNow.AddDays(5);
        var reviewDeadline = DateTimeOffset.UtcNow.AddDays(10);
        var homeworkId = await AddHomeworkInDbAsync(
            courseId,
            teacherId,
            HomeworkStatusModel.Published,
            name: homeworkName,
            description: homeworkDescription,
            deadline: deadline,
            reviewDeadline: reviewDeadline,
            amountOfReviewers: 4,
            rubricId: rubricId);
        var firstFile = await AddHomeworkFileInDbAsync(homeworkId, teacherId, _fixture.Create<string>(), 1024);
        var secondFile = await AddHomeworkFileInDbAsync(homeworkId, teacherId, _fixture.Create<string>(), 2048);

        var request = _fixture.Build<V1GetStudentHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await GetStudentHomeworkClient.V1GetStudentHomeworkAsync(request);

        // Assert
        response.StudentHomeworkInfo.Id.Should().Be(homeworkId);
        response.StudentHomeworkInfo.Name.Should().Be(homeworkName);
        response.StudentHomeworkInfo.Description.Should().Be(homeworkDescription);
        response.StudentHomeworkInfo.Status.Should().Be(ProtoHomeworkStatus.Published);
        response.StudentHomeworkInfo.AmountOfReviewers.Should().Be(4);
        response.StudentHomeworkInfo.HasRubricId.Should().BeTrue();
        response.StudentHomeworkInfo.RubricId.Should().Be(rubricId);
        response.StudentHomeworkInfo.IsHomeworkSubmitted.Should().BeFalse();
        response.HasSubmittedHomeworkId.Should().BeFalse();
        response.HomeworkFiles
            .Select(file => new { file.Id, file.Name, file.Size })
            .Should()
            .BeEquivalentTo(
            [
                new { firstFile.Id, firstFile.Name, firstFile.Size },
                new { secondFile.Id, secondFile.Name, secondFile.Size }
            ]);
    }

    [Fact]
    public async Task V1GetStudentHomework_SubmittedHomeworkExists_ShouldReturnSubmittedHomeworkId()
    {
        // Arrange
        var studentId = _fixture.Create<long>();
        var teacherId = _fixture.Create<long>();
        await AddStudentInDbAsync(studentId);
        await AddTeacherInDbAsync(teacherId);

        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);
        await AddGroupStudentInDbAsync(groupId, studentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);

        var request = _fixture.Build<V1GetStudentHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await GetStudentHomeworkClient.V1GetStudentHomeworkAsync(request);

        // Assert
        response.HasSubmittedHomeworkId.Should().BeTrue();
        response.SubmittedHomeworkId.Should().Be(submittedHomeworkId);
        response.StudentHomeworkInfo.IsHomeworkSubmitted.Should().BeTrue();
    }

    [Fact]
    public async Task V1GetStudentHomework_GroupStudentExists_ShouldReturnGroupHomework()
    {
        // Arrange
        var studentId = _fixture.Create<long>();
        var teacherId = _fixture.Create<long>();
        await AddStudentInDbAsync(studentId);
        await AddTeacherInDbAsync(teacherId);

        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);
        await AddGroupStudentInDbAsync(groupId, studentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Reviewing, groupId: groupId);

        var request = _fixture.Build<V1GetStudentHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await GetStudentHomeworkClient.V1GetStudentHomeworkAsync(request);

        // Assert
        response.StudentHomeworkInfo.Id.Should().Be(homeworkId);
        response.StudentHomeworkInfo.Status.Should().Be(ProtoHomeworkStatus.Reviewing);
    }

    [Fact]
    public async Task V1GetStudentHomework_DraftHomework_ShouldReturnNotFound()
    {
        // Arrange
        var studentId = _fixture.Create<long>();
        var teacherId = _fixture.Create<long>();
        await AddStudentInDbAsync(studentId);
        await AddTeacherInDbAsync(teacherId);

        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);
        await AddGroupStudentInDbAsync(groupId, studentId);
        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Draft);

        var request = _fixture.Build<V1GetStudentHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var act = async () => await GetStudentHomeworkClient.V1GetStudentHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task V1GetStudentHomework_StudentHasNoAccess_ShouldReturnNotFound()
    {
        // Arrange
        var studentId = _fixture.Create<long>();
        var otherStudentId = studentId + 1;
        var teacherId = _fixture.Create<long>();
        await AddStudentInDbAsync(studentId);
        await AddStudentInDbAsync(otherStudentId);
        await AddTeacherInDbAsync(teacherId);

        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);
        await AddGroupStudentInDbAsync(groupId, otherStudentId);
        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published, groupId: groupId);

        var request = _fixture.Build<V1GetStudentHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var act = async () => await GetStudentHomeworkClient.V1GetStudentHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetStudentHomework_NotPositiveHomeworkId_ShouldReturnInvalidArgument(long homeworkId)
    {
        // Arrange
        var request = _fixture.Build<V1GetStudentHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, _fixture.Create<long>())
            .Create();

        // Act
        var act = async () => await GetStudentHomeworkClient.V1GetStudentHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.HomeworkId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetStudentHomework_NotPositiveStudentId_ShouldReturnInvalidArgument(long studentId)
    {
        // Arrange
        var request = _fixture.Build<V1GetStudentHomeworkRequest>()
            .With(result => result.HomeworkId, _fixture.Create<long>())
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var act = async () => await GetStudentHomeworkClient.V1GetStudentHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StudentId));
    }
}
