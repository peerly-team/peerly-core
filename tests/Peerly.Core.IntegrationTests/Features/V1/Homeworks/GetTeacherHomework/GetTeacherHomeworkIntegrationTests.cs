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

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.GetTeacherHomework;

public sealed class GetTeacherHomeworkIntegrationTests : GetTeacherHomeworkIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public GetTeacherHomeworkIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1GetTeacherHomework_CourseTeacherExists_ShouldReturnTeacherHomework()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var firstStudentId = teacherId + 1;
        var secondStudentId = teacherId + 2;
        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(firstStudentId);
        await AddStudentInDbAsync(secondStudentId);

        var courseId = await AddCourseInDbAsync();
        await AddCourseTeacherInDbAsync(courseId, teacherId);

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
            discrepancyThreshold: 15,
            rubricId: rubricId);
        var firstFile = await AddHomeworkFileInDbAsync(homeworkId, teacherId, _fixture.Create<string>(), 1024);
        var secondFile = await AddHomeworkFileInDbAsync(homeworkId, teacherId, _fixture.Create<string>(), 2048);
        await AddSubmittedHomeworkInDbAsync(homeworkId, firstStudentId);
        await AddSubmittedHomeworkInDbAsync(homeworkId, secondStudentId);

        var request = _fixture.Build<V1GetTeacherHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var response = await GetTeacherHomeworkClient.V1GetTeacherHomeworkAsync(request);

        // Assert
        response.TeacherHomeworkInfo.Id.Should().Be(homeworkId);
        response.TeacherHomeworkInfo.Name.Should().Be(homeworkName);
        response.TeacherHomeworkInfo.Description.Should().Be(homeworkDescription);
        response.TeacherHomeworkInfo.Status.Should().Be(ProtoHomeworkStatus.Published);
        response.TeacherHomeworkInfo.Deadline.ToDateTimeOffset().Should().BeCloseTo(deadline, TimeSpan.FromMilliseconds(1));
        response.TeacherHomeworkInfo.ReviewDeadline.ToDateTimeOffset().Should().BeCloseTo(reviewDeadline, TimeSpan.FromMilliseconds(1));
        response.TeacherHomeworkInfo.AmountOfReviewers.Should().Be(4);
        response.TeacherHomeworkInfo.DiscrepancyThreshold.Should().Be(15);
        response.TeacherHomeworkInfo.HasRubricId.Should().BeTrue();
        response.TeacherHomeworkInfo.RubricId.Should().Be(rubricId);
        response.HasSubmittedCount.Should().BeTrue();
        response.SubmittedCount.Should().Be(2);
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
    public async Task V1GetTeacherHomework_GroupTeacherExists_ShouldReturnTeacherHomework()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var homeworkTeacherId = teacherId + 1;
        await AddTeacherInDbAsync(teacherId);
        await AddTeacherInDbAsync(homeworkTeacherId);

        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);
        await AddGroupTeacherInDbAsync(groupId, teacherId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, homeworkTeacherId, HomeworkStatusModel.Reviewing, groupId: groupId);

        var request = _fixture.Build<V1GetTeacherHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var response = await GetTeacherHomeworkClient.V1GetTeacherHomeworkAsync(request);

        // Assert
        response.TeacherHomeworkInfo.Id.Should().Be(homeworkId);
        response.TeacherHomeworkInfo.Status.Should().Be(ProtoHomeworkStatus.Reviewing);
        response.HasSubmittedCount.Should().BeTrue();
        response.SubmittedCount.Should().Be(0);
    }

    [Fact]
    public async Task V1GetTeacherHomework_DraftHomework_ShouldNotReturnSubmittedCount()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        await AddTeacherInDbAsync(teacherId);

        var courseId = await AddCourseInDbAsync();
        await AddCourseTeacherInDbAsync(courseId, teacherId);
        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Draft);

        var request = _fixture.Build<V1GetTeacherHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var response = await GetTeacherHomeworkClient.V1GetTeacherHomeworkAsync(request);

        // Assert
        response.TeacherHomeworkInfo.Id.Should().Be(homeworkId);
        response.TeacherHomeworkInfo.Status.Should().Be(ProtoHomeworkStatus.Draft);
        response.HasSubmittedCount.Should().BeFalse();
    }

    [Fact]
    public async Task V1GetTeacherHomework_HomeworkNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var request = _fixture.Build<V1GetTeacherHomeworkRequest>()
            .With(result => result.HomeworkId, _fixture.Create<long>())
            .With(result => result.TeacherId, _fixture.Create<long>())
            .Create();

        // Act
        var act = async () => await GetTeacherHomeworkClient.V1GetTeacherHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task V1GetTeacherHomework_TeacherHasNoAccess_ShouldReturnNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var otherTeacherId = teacherId + 1;
        await AddTeacherInDbAsync(teacherId);
        await AddTeacherInDbAsync(otherTeacherId);

        var courseId = await AddCourseInDbAsync();
        await AddCourseTeacherInDbAsync(courseId, otherTeacherId);
        var homeworkId = await AddHomeworkInDbAsync(courseId, otherTeacherId, HomeworkStatusModel.Published);

        var request = _fixture.Build<V1GetTeacherHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var act = async () => await GetTeacherHomeworkClient.V1GetTeacherHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetTeacherHomework_NotPositiveHomeworkId_ShouldReturnInvalidArgument(long homeworkId)
    {
        // Arrange
        var request = _fixture.Build<V1GetTeacherHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.TeacherId, _fixture.Create<long>())
            .Create();

        // Act
        var act = async () => await GetTeacherHomeworkClient.V1GetTeacherHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.HomeworkId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetTeacherHomework_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = _fixture.Build<V1GetTeacherHomeworkRequest>()
            .With(result => result.HomeworkId, _fixture.Create<long>())
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var act = async () => await GetTeacherHomeworkClient.V1GetTeacherHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }
}
