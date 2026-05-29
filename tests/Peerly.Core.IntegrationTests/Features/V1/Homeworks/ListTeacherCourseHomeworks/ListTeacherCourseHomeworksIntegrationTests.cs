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

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.ListTeacherCourseHomeworks;

public sealed class ListTeacherCourseHomeworksIntegrationTests : ListTeacherCourseHomeworksIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public ListTeacherCourseHomeworksIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1ListTeacherCourseHomeworks_CourseTeacherHasCourseHomeworks_ShouldReturnTeacherHomeworks()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var otherTeacherId = teacherId + 1;
        await AddTeacherInDbAsync(teacherId);
        await AddTeacherInDbAsync(otherTeacherId);

        var courseId = await AddCourseInDbAsync();
        var firstGroupId = await AddGroupInDbAsync(courseId);
        var secondGroupId = await AddGroupInDbAsync(courseId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        var otherCourseId = await AddCourseInDbAsync();
        await AddCourseTeacherInDbAsync(otherCourseId, teacherId);

        var rubricId = await AddRubricInDbAsync(teacherId);
        var homeworkName = _fixture.Create<string>();
        var homeworkDescription = _fixture.Create<string>();
        var homeworkDeadline = DateTimeOffset.UtcNow.AddDays(10);
        var homeworkReviewDeadline = DateTimeOffset.UtcNow.AddDays(15);
        var courseHomeworkId = await AddHomeworkInDbAsync(
            courseId,
            teacherId,
            HomeworkStatusModel.Published,
            name: homeworkName,
            description: homeworkDescription,
            deadline: homeworkDeadline,
            reviewDeadline: homeworkReviewDeadline,
            amountOfReviewers: 4,
            discrepancyThreshold: 15,
            rubricId: rubricId);
        var firstGroupHomeworkId = await AddHomeworkInDbAsync(courseId, otherTeacherId, HomeworkStatusModel.Reviewing, groupId: firstGroupId);
        var secondGroupHomeworkId = await AddHomeworkInDbAsync(courseId, otherTeacherId, HomeworkStatusModel.Draft, groupId: secondGroupId);
        await AddHomeworkInDbAsync(otherCourseId, teacherId, HomeworkStatusModel.Published);

        var request = _fixture.Build<V1ListTeacherCourseHomeworksRequest>()
            .With(result => result.TeacherId, teacherId)
            .With(result => result.CourseId, courseId)
            .Create();

        // Act
        var response = await ListTeacherCourseHomeworksClient.V1ListTeacherCourseHomeworksAsync(request);

        // Assert
        response.TeacherHomeworkInfos.Select(homeworkInfo => homeworkInfo.Id)
            .Should()
            .BeEquivalentTo([courseHomeworkId, firstGroupHomeworkId, secondGroupHomeworkId]);

        var courseHomeworkInfo = response.TeacherHomeworkInfos.Single(homeworkInfo => homeworkInfo.Id == courseHomeworkId);
        courseHomeworkInfo.Name.Should().Be(homeworkName);
        courseHomeworkInfo.Description.Should().Be(homeworkDescription);
        courseHomeworkInfo.Status.Should().Be(ProtoHomeworkStatus.Published);
        courseHomeworkInfo.Deadline.ToDateTimeOffset().Should().BeCloseTo(homeworkDeadline, TimeSpan.FromMilliseconds(1));
        courseHomeworkInfo.ReviewDeadline.ToDateTimeOffset().Should().BeCloseTo(homeworkReviewDeadline, TimeSpan.FromMilliseconds(1));
        courseHomeworkInfo.AmountOfReviewers.Should().Be(4);
        courseHomeworkInfo.DiscrepancyThreshold.Should().Be(15);
        courseHomeworkInfo.HasRubricId.Should().BeTrue();
        courseHomeworkInfo.RubricId.Should().Be(rubricId);
    }

    [Fact]
    public async Task V1ListTeacherCourseHomeworks_GroupTeacherHasCourseHomeworks_ShouldReturnTeacherHomeworks()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var homeworkTeacherId = teacherId + 1;
        await AddTeacherInDbAsync(teacherId);
        await AddTeacherInDbAsync(homeworkTeacherId);

        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);
        await AddGroupTeacherInDbAsync(groupId, teacherId);

        var courseHomeworkId = await AddHomeworkInDbAsync(courseId, homeworkTeacherId, HomeworkStatusModel.Published);
        var groupHomeworkId = await AddHomeworkInDbAsync(courseId, homeworkTeacherId, HomeworkStatusModel.Reviewing, groupId: groupId);

        var request = _fixture.Build<V1ListTeacherCourseHomeworksRequest>()
            .With(result => result.TeacherId, teacherId)
            .With(result => result.CourseId, courseId)
            .Create();

        // Act
        var response = await ListTeacherCourseHomeworksClient.V1ListTeacherCourseHomeworksAsync(request);

        // Assert
        response.TeacherHomeworkInfos.Select(homeworkInfo => homeworkInfo.Id)
            .Should()
            .BeEquivalentTo([courseHomeworkId, groupHomeworkId]);
    }

    [Fact]
    public async Task V1ListTeacherCourseHomeworks_TeacherHasNoCourseAccess_ShouldReturnEmptyTeacherHomeworkInfos()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var otherTeacherId = teacherId + 1;
        await AddTeacherInDbAsync(teacherId);
        await AddTeacherInDbAsync(otherTeacherId);

        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);
        await AddGroupTeacherInDbAsync(groupId, otherTeacherId);
        await AddHomeworkInDbAsync(courseId, otherTeacherId, HomeworkStatusModel.Published);

        var request = _fixture.Build<V1ListTeacherCourseHomeworksRequest>()
            .With(result => result.TeacherId, teacherId)
            .With(result => result.CourseId, courseId)
            .Create();

        // Act
        var response = await ListTeacherCourseHomeworksClient.V1ListTeacherCourseHomeworksAsync(request);

        // Assert
        response.TeacherHomeworkInfos.Should().BeEmpty();
    }

    [Fact]
    public async Task V1ListTeacherCourseHomeworks_ShouldReturnHomeworksOrderedByDeadlineDesc()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        await AddTeacherInDbAsync(teacherId);

        var courseId = await AddCourseInDbAsync();
        await AddCourseTeacherInDbAsync(courseId, teacherId);

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
            deadline: DateTimeOffset.UtcNow.AddDays(3));

        var request = _fixture.Build<V1ListTeacherCourseHomeworksRequest>()
            .With(result => result.TeacherId, teacherId)
            .With(result => result.CourseId, courseId)
            .Create();

        // Act
        var response = await ListTeacherCourseHomeworksClient.V1ListTeacherCourseHomeworksAsync(request);

        // Assert
        response.TeacherHomeworkInfos.Select(homeworkInfo => homeworkInfo.Id).Should().Equal([thirdHomeworkId, secondHomeworkId, firstHomeworkId]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1ListTeacherCourseHomeworks_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = _fixture.Build<V1ListTeacherCourseHomeworksRequest>()
            .With(result => result.TeacherId, teacherId)
            .With(result => result.CourseId, _fixture.Create<long>())
            .Create();

        // Act
        var act = async () => await ListTeacherCourseHomeworksClient.V1ListTeacherCourseHomeworksAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1ListTeacherCourseHomeworks_NotPositiveCourseId_ShouldReturnInvalidArgument(long courseId)
    {
        // Arrange
        var request = _fixture.Build<V1ListTeacherCourseHomeworksRequest>()
            .With(result => result.TeacherId, _fixture.Create<long>())
            .With(result => result.CourseId, courseId)
            .Create();

        // Act
        var act = async () => await ListTeacherCourseHomeworksClient.V1ListTeacherCourseHomeworksAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.CourseId));
    }
}
