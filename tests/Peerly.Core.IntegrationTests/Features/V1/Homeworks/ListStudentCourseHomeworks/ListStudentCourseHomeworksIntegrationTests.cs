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

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.ListStudentCourseHomeworks;

public sealed class ListStudentCourseHomeworksIntegrationTests : ListStudentCourseHomeworksIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public ListStudentCourseHomeworksIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1ListStudentCourseHomeworks_StudentHasCourseHomeworks_ShouldReturnVisibleStudentHomeworks()
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
        var otherGroupId = await AddGroupInDbAsync(courseId);
        await AddGroupStudentInDbAsync(groupId, studentId);
        await AddGroupStudentInDbAsync(otherGroupId, otherStudentId);

        var otherCourseId = await AddCourseInDbAsync();
        var otherCourseGroupId = await AddGroupInDbAsync(otherCourseId);
        await AddGroupStudentInDbAsync(otherCourseGroupId, studentId);

        var publishedHomeworkName = _fixture.Create<string>();
        var publishedHomeworkDescription = _fixture.Create<string>();
        var publishedHomeworkChecklist = _fixture.Create<string>();
        var publishedHomeworkDeadline = DateTimeOffset.UtcNow.AddDays(10);
        var publishedHomeworkReviewDeadline = DateTimeOffset.UtcNow.AddDays(15);
        var publishedHomeworkId = await AddHomeworkInDbAsync(
            courseId,
            teacherId,
            HomeworkStatusModel.Published,
            name: publishedHomeworkName,
            description: publishedHomeworkDescription,
            checklist: publishedHomeworkChecklist,
            deadline: publishedHomeworkDeadline,
            reviewDeadline: publishedHomeworkReviewDeadline,
            amountOfReviewers: 4);
        var reviewingHomeworkId = await AddHomeworkInDbAsync(
            courseId,
            teacherId,
            HomeworkStatusModel.Reviewing,
            groupId: groupId,
            deadline: DateTimeOffset.UtcNow.AddDays(5));
        await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published, groupId: otherGroupId);
        await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Draft);
        await AddHomeworkInDbAsync(otherCourseId, teacherId, HomeworkStatusModel.Published);
        await AddSubmittedHomeworkInDbAsync(publishedHomeworkId, studentId);

        var request = _fixture.Build<V1ListStudentCourseHomeworksRequest>()
            .With(result => result.StudentId, studentId)
            .With(result => result.CourseId, courseId)
            .Create();

        // Act
        var response = await ListStudentCourseHomeworksClient.V1ListStudentCourseHomeworksAsync(request);

        // Assert
        response.StudentHomeworkInfos.Select(homeworkInfo => homeworkInfo.Id)
            .Should()
            .BeEquivalentTo([publishedHomeworkId, reviewingHomeworkId]);

        var publishedHomeworkInfo = response.StudentHomeworkInfos.Single(homeworkInfo => homeworkInfo.Id == publishedHomeworkId);
        publishedHomeworkInfo.Name.Should().Be(publishedHomeworkName);
        publishedHomeworkInfo.Description.Should().Be(publishedHomeworkDescription);
        publishedHomeworkInfo.Checklist.Should().Be(publishedHomeworkChecklist);
        publishedHomeworkInfo.Status.Should().Be(ProtoHomeworkStatus.Published);
        publishedHomeworkInfo.AmountOfReviewers.Should().Be(4);
        publishedHomeworkInfo.IsHomeworkSubmitted.Should().BeTrue();

        var reviewingHomeworkInfo = response.StudentHomeworkInfos.Single(homeworkInfo => homeworkInfo.Id == reviewingHomeworkId);
        reviewingHomeworkInfo.Status.Should().Be(ProtoHomeworkStatus.Reviewing);
        reviewingHomeworkInfo.IsHomeworkSubmitted.Should().BeFalse();
    }

    [Fact]
    public async Task V1ListStudentCourseHomeworks_StudentHasNoCourseGroup_ShouldReturnEmptyStudentHomeworkInfos()
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
        await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published);

        var request = _fixture.Build<V1ListStudentCourseHomeworksRequest>()
            .With(result => result.StudentId, studentId)
            .With(result => result.CourseId, courseId)
            .Create();

        // Act
        var response = await ListStudentCourseHomeworksClient.V1ListStudentCourseHomeworksAsync(request);

        // Assert
        response.StudentHomeworkInfos.Should().BeEmpty();
    }

    [Fact]
    public async Task V1ListStudentCourseHomeworks_ShouldReturnHomeworksOrderedByDeadlineDesc()
    {
        // Arrange
        var studentId = _fixture.Create<long>();
        var teacherId = _fixture.Create<long>();
        await AddStudentInDbAsync(studentId);
        await AddTeacherInDbAsync(teacherId);

        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);
        await AddGroupStudentInDbAsync(groupId, studentId);

        var firstHomeworkId = await AddHomeworkInDbAsync(
            courseId,
            teacherId,
            HomeworkStatusModel.Published,
            deadline: DateTimeOffset.UtcNow.AddDays(1));
        var secondHomeworkId = await AddHomeworkInDbAsync(
            courseId,
            teacherId,
            HomeworkStatusModel.Published,
            deadline: DateTimeOffset.UtcNow.AddDays(2));
        var thirdHomeworkId = await AddHomeworkInDbAsync(
            courseId,
            teacherId,
            HomeworkStatusModel.Published,
            groupId: groupId,
            deadline: DateTimeOffset.UtcNow.AddDays(3));

        var request = _fixture.Build<V1ListStudentCourseHomeworksRequest>()
            .With(result => result.StudentId, studentId)
            .With(result => result.CourseId, courseId)
            .Create();

        // Act
        var response = await ListStudentCourseHomeworksClient.V1ListStudentCourseHomeworksAsync(request);

        // Assert
        response.StudentHomeworkInfos.Select(homeworkInfo => homeworkInfo.Id).Should().Equal(thirdHomeworkId, secondHomeworkId, firstHomeworkId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1ListStudentCourseHomeworks_NotPositiveStudentId_ShouldReturnInvalidArgument(long studentId)
    {
        // Arrange
        var request = _fixture.Build<V1ListStudentCourseHomeworksRequest>()
            .With(result => result.StudentId, studentId)
            .With(result => result.CourseId, _fixture.Create<long>())
            .Create();

        // Act
        var act = async () => await ListStudentCourseHomeworksClient.V1ListStudentCourseHomeworksAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StudentId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1ListStudentCourseHomeworks_NotPositiveCourseId_ShouldReturnInvalidArgument(long courseId)
    {
        // Arrange
        var request = _fixture.Build<V1ListStudentCourseHomeworksRequest>()
            .With(result => result.StudentId, _fixture.Create<long>())
            .With(result => result.CourseId, courseId)
            .Create();

        // Act
        var act = async () => await ListStudentCourseHomeworksClient.V1ListStudentCourseHomeworksAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.CourseId));
    }
}
