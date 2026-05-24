using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;
using HomeworkStatusModel = Peerly.Core.Models.Homeworks.HomeworkStatus;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.GetTeacherSubmittedHomework;

public sealed class GetTeacherSubmittedHomeworkIntegrationTests : GetTeacherSubmittedHomeworkIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public GetTeacherSubmittedHomeworkIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1GetTeacherSubmittedHomework_CourseTeacherExists_ShouldReturnSubmittedHomework()
    {
        // Arrange
        var teacherId = _fixture.Create<int>();
        var ownerStudentId = teacherId + 1;
        var reviewerStudentId = teacherId + 2;
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(reviewerStudentId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Finished);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId);
        var file = await AddFileInDbAsync(_fixture.Create<string>(), 1024);
        await AddSubmittedHomeworkFileInDbAsync(submittedHomeworkId, file.Id);
        var submittedReviewId = await AddSubmittedReviewInDbAsync(submittedHomeworkId, reviewerStudentId, mark: 81);
        await AddSubmittedHomeworkMarkInDbAsync(submittedHomeworkId, reviewersMark: 81, teacherMark: 95);

        var request = _fixture.Build<V1GetTeacherSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var response = await GetTeacherSubmittedHomeworkClient.V1GetTeacherSubmittedHomeworkAsync(request);

        // Assert
        response.SubmittedHomework.Id.Should().Be(submittedHomeworkId);
        response.SubmittedHomework.Comment.Should().Be("Test comment");
        response.SubmittedHomework.Files.Should().ContainSingle().Which
            .Should().BeEquivalentTo(new { file.Id, file.Name, file.Size });
        response.Student.Should().BeEquivalentTo(new
        {
            StudentId = ownerStudentId,
            Email = $"student-{ownerStudentId}@peerly.test",
            Name = $"Student {ownerStudentId}"
        });

        var review = response.SubmittedReviews.Should().ContainSingle().Which;
        review.SubmittedReview.Should().BeEquivalentTo(new { Id = submittedReviewId, Mark = 81, Comment = "Review comment" });
        review.Reviewer.Should().BeEquivalentTo(new
        {
            StudentId = reviewerStudentId,
            Email = $"student-{reviewerStudentId}@peerly.test",
            Name = $"Student {reviewerStudentId}"
        });
        response.ReviewersMark.Should().Be(81);
        response.TeacherMark.Should().Be(95);
    }

    [Fact]
    public async Task V1GetTeacherSubmittedHomework_GroupTeacherExists_ShouldReturnSubmittedHomework()
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

        var request = _fixture.Build<V1GetTeacherSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var response = await GetTeacherSubmittedHomeworkClient.V1GetTeacherSubmittedHomeworkAsync(request);

        // Assert
        response.SubmittedHomework.Id.Should().Be(submittedHomeworkId);
        response.Student.StudentId.Should().Be(studentId);
    }

    [Fact]
    public async Task V1GetTeacherSubmittedHomework_HomeworkReviewing_ShouldNotSetMarks()
    {
        // Arrange
        var teacherId = _fixture.Create<int>();
        var ownerStudentId = teacherId + 1;
        var reviewerStudentId = teacherId + 2;
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(reviewerStudentId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Reviewing);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId);
        await AddSubmittedReviewInDbAsync(submittedHomeworkId, reviewerStudentId, mark: 81);
        await AddSubmittedHomeworkMarkInDbAsync(submittedHomeworkId, reviewersMark: 81, teacherMark: 95);

        var request = _fixture.Build<V1GetTeacherSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var response = await GetTeacherSubmittedHomeworkClient.V1GetTeacherSubmittedHomeworkAsync(request);

        // Assert
        response.SubmittedHomework.Id.Should().Be(submittedHomeworkId);
        response.SubmittedReviews.Should().ContainSingle();
        response.HasReviewersMark.Should().BeFalse();
        response.HasTeacherMark.Should().BeFalse();
    }

    [Fact]
    public async Task V1GetTeacherSubmittedHomework_SubmittedHomeworkNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var request = _fixture.Build<V1GetTeacherSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, _fixture.Create<int>())
            .With(result => result.TeacherId, _fixture.Create<int>())
            .Create();

        // Act
        var action = async () => await GetTeacherSubmittedHomeworkClient.V1GetTeacherSubmittedHomeworkAsync(request);

        // Assert
        var exception = await action.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task V1GetTeacherSubmittedHomework_TeacherHasNoAccess_ShouldReturnNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<int>();
        var otherTeacherId = teacherId + 1;
        var studentId = teacherId + 2;
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddTeacherInDbAsync(otherTeacherId);
        await AddStudentInDbAsync(studentId);
        await AddCourseTeacherInDbAsync(courseId, otherTeacherId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, otherTeacherId, HomeworkStatusModel.Finished);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);

        var request = _fixture.Build<V1GetTeacherSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var action = async () => await GetTeacherSubmittedHomeworkClient.V1GetTeacherSubmittedHomeworkAsync(request);

        // Assert
        var exception = await action.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task V1GetTeacherSubmittedHomework_HomeworkNotVisibleToTeacher_ShouldReturnNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<int>();
        var studentId = teacherId + 1;
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);

        var request = _fixture.Build<V1GetTeacherSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var action = async () => await GetTeacherSubmittedHomeworkClient.V1GetTeacherSubmittedHomeworkAsync(request);

        // Assert
        var exception = await action.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetTeacherSubmittedHomework_NotPositiveSubmittedHomeworkId_ShouldReturnInvalidArgument(long submittedHomeworkId)
    {
        // Arrange
        var request = _fixture.Build<V1GetTeacherSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.TeacherId, _fixture.Create<int>())
            .Create();

        // Act
        var action = async () => await GetTeacherSubmittedHomeworkClient.V1GetTeacherSubmittedHomeworkAsync(request);

        // Assert
        var exception = await action.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.SubmittedHomeworkId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetTeacherSubmittedHomework_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = _fixture.Build<V1GetTeacherSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, _fixture.Create<int>())
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var action = async () => await GetTeacherSubmittedHomeworkClient.V1GetTeacherSubmittedHomeworkAsync(request);

        // Assert
        var exception = await action.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }
}
