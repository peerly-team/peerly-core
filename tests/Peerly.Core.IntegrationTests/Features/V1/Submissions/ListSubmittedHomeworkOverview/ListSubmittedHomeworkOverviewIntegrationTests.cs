using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;
using HomeworkStatusModel = Peerly.Core.Models.Homeworks.HomeworkStatus;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.ListSubmittedHomeworkOverview;

public sealed class ListSubmittedHomeworkOverviewIntegrationTests : ListSubmittedHomeworkOverviewIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public ListSubmittedHomeworkOverviewIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1ListSubmittedHomeworkOverview_CourseTeacherExists_ShouldReturnSubmittedHomeworkOverviews()
    {
        // Arrange
        var teacherId = _fixture.Create<int>();
        var firstStudentId = teacherId + 1;
        var secondStudentId = teacherId + 2;
        var firstReviewerStudentId = teacherId + 3;
        var secondReviewerStudentId = teacherId + 4;
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(firstStudentId);
        await AddStudentInDbAsync(secondStudentId);
        await AddStudentInDbAsync(firstReviewerStudentId);
        await AddStudentInDbAsync(secondReviewerStudentId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Finished);
        var firstSubmittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, firstStudentId);
        var secondSubmittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, secondStudentId);
        await AddSubmittedReviewInDbAsync(firstSubmittedHomeworkId, firstReviewerStudentId, mark: 80);
        await AddSubmittedReviewInDbAsync(firstSubmittedHomeworkId, secondReviewerStudentId, mark: 90);
        await AddSubmittedReviewInDbAsync(secondSubmittedHomeworkId, firstReviewerStudentId, mark: 70);
        await AddSubmittedHomeworkMarkInDbAsync(firstSubmittedHomeworkId, reviewersMark: 85, teacherMark: 95, hasDiscrepancy: true);

        var request = _fixture.Build<V1ListSubmittedHomeworkOverviewRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var response = await ListSubmittedHomeworkOverviewClient.V1ListSubmittedHomeworkOverviewAsync(request);

        // Assert
        response.SubmittedHomeworkResults.Should().HaveCount(2);

        var firstOverview = response.SubmittedHomeworkResults.Single(result => result.Id == firstSubmittedHomeworkId);
        firstOverview.Student.Should().BeEquivalentTo(new
        {
            StudentId = firstStudentId,
            Email = $"student-{firstStudentId}@peerly.test",
            Name = $"Student {firstStudentId}"
        });
        firstOverview.ReviewCount.Should().Be(2);
        firstOverview.HasReviewersMark.Should().BeTrue();
        firstOverview.ReviewersMark.Should().Be(85);
        firstOverview.HasHasDiscrepancy.Should().BeTrue();
        firstOverview.HasDiscrepancy.Should().BeTrue();
        firstOverview.HasTeacherMark.Should().BeTrue();
        firstOverview.TeacherMark.Should().Be(95);

        var secondOverview = response.SubmittedHomeworkResults.Single(result => result.Id == secondSubmittedHomeworkId);
        secondOverview.Student.StudentId.Should().Be(secondStudentId);
        secondOverview.ReviewCount.Should().Be(1);
        secondOverview.HasReviewersMark.Should().BeFalse();
        secondOverview.HasHasDiscrepancy.Should().BeFalse();
        secondOverview.HasTeacherMark.Should().BeFalse();
    }

    [Fact]
    public async Task V1ListSubmittedHomeworkOverview_GroupTeacherExists_ShouldReturnSubmittedHomeworkOverviews()
    {
        // Arrange
        var teacherId = _fixture.Create<int>();
        var homeworkTeacherId = teacherId + 1;
        var studentId = teacherId + 2;
        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);

        await AddTeacherInDbAsync(teacherId);
        await AddTeacherInDbAsync(homeworkTeacherId);
        await AddStudentInDbAsync(studentId);
        await AddGroupTeacherInDbAsync(groupId, teacherId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, homeworkTeacherId, HomeworkStatusModel.Confirmation, groupId: groupId);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);

        var request = _fixture.Build<V1ListSubmittedHomeworkOverviewRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var response = await ListSubmittedHomeworkOverviewClient.V1ListSubmittedHomeworkOverviewAsync(request);

        // Assert
        var overview = response.SubmittedHomeworkResults.Should().ContainSingle().Which;
        overview.Id.Should().Be(submittedHomeworkId);
        overview.Student.StudentId.Should().Be(studentId);
    }

    [Fact]
    public async Task V1ListSubmittedHomeworkOverview_HomeworkReviewing_ShouldNotSetMarks()
    {
        // Arrange
        var teacherId = _fixture.Create<int>();
        var studentId = teacherId + 1;
        var reviewerStudentId = teacherId + 2;
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);
        await AddStudentInDbAsync(reviewerStudentId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Reviewing);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);
        await AddSubmittedReviewInDbAsync(submittedHomeworkId, reviewerStudentId, mark: 80);
        await AddSubmittedHomeworkMarkInDbAsync(submittedHomeworkId, reviewersMark: 80, teacherMark: 95, hasDiscrepancy: true);

        var request = _fixture.Build<V1ListSubmittedHomeworkOverviewRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var response = await ListSubmittedHomeworkOverviewClient.V1ListSubmittedHomeworkOverviewAsync(request);

        // Assert
        var overview = response.SubmittedHomeworkResults.Should().ContainSingle().Which;
        overview.Id.Should().Be(submittedHomeworkId);
        overview.ReviewCount.Should().Be(1);
        overview.HasReviewersMark.Should().BeFalse();
        overview.HasHasDiscrepancy.Should().BeFalse();
        overview.HasTeacherMark.Should().BeFalse();
    }

    [Fact]
    public async Task V1ListSubmittedHomeworkOverview_NoSubmittedHomework_ShouldReturnEmptyCollection()
    {
        // Arrange
        var teacherId = _fixture.Create<int>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Finished);
        var request = _fixture.Build<V1ListSubmittedHomeworkOverviewRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var response = await ListSubmittedHomeworkOverviewClient.V1ListSubmittedHomeworkOverviewAsync(request);

        // Assert
        response.SubmittedHomeworkResults.Should().BeEmpty();
    }

    [Fact]
    public async Task V1ListSubmittedHomeworkOverview_HomeworkNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var request = _fixture.Build<V1ListSubmittedHomeworkOverviewRequest>()
            .With(result => result.HomeworkId, _fixture.Create<int>())
            .With(result => result.TeacherId, _fixture.Create<int>())
            .Create();

        // Act
        var action = async () => await ListSubmittedHomeworkOverviewClient.V1ListSubmittedHomeworkOverviewAsync(request);

        // Assert
        var exception = await action.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task V1ListSubmittedHomeworkOverview_TeacherHasNoAccess_ShouldReturnNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<int>();
        var otherTeacherId = teacherId + 1;
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddTeacherInDbAsync(otherTeacherId);
        await AddCourseTeacherInDbAsync(courseId, otherTeacherId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, otherTeacherId, HomeworkStatusModel.Finished);
        var request = _fixture.Build<V1ListSubmittedHomeworkOverviewRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var action = async () => await ListSubmittedHomeworkOverviewClient.V1ListSubmittedHomeworkOverviewAsync(request);

        // Assert
        var exception = await action.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task V1ListSubmittedHomeworkOverview_HomeworkNotVisibleToTeacher_ShouldReturnNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<int>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published);
        var request = _fixture.Build<V1ListSubmittedHomeworkOverviewRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var action = async () => await ListSubmittedHomeworkOverviewClient.V1ListSubmittedHomeworkOverviewAsync(request);

        // Assert
        var exception = await action.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1ListSubmittedHomeworkOverview_NotPositiveHomeworkId_ShouldReturnInvalidArgument(long homeworkId)
    {
        // Arrange
        var request = _fixture.Build<V1ListSubmittedHomeworkOverviewRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.TeacherId, _fixture.Create<int>())
            .Create();

        // Act
        var action = async () => await ListSubmittedHomeworkOverviewClient.V1ListSubmittedHomeworkOverviewAsync(request);

        // Assert
        var exception = await action.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.HomeworkId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1ListSubmittedHomeworkOverview_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = _fixture.Build<V1ListSubmittedHomeworkOverviewRequest>()
            .With(result => result.HomeworkId, _fixture.Create<int>())
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var action = async () => await ListSubmittedHomeworkOverviewClient.V1ListSubmittedHomeworkOverviewAsync(request);

        // Assert
        var exception = await action.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }
}
