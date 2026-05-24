using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;
using HomeworkStatusModel = Peerly.Core.Models.Homeworks.HomeworkStatus;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.ListAssignedReviews;

public sealed class ListAssignedReviewsIntegrationTests : ListAssignedReviewsIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public ListAssignedReviewsIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1ListAssignedReviews_CourseStudentHasAssignedReviews_ShouldReturnAssignedReviews()
    {
        // Arrange
        var teacherId = _fixture.Create<int>();
        var reviewerStudentId = teacherId + 1;
        var firstOwnerStudentId = teacherId + 2;
        var secondOwnerStudentId = teacherId + 3;
        var otherReviewerStudentId = teacherId + 4;
        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(reviewerStudentId);
        await AddStudentInDbAsync(firstOwnerStudentId);
        await AddStudentInDbAsync(secondOwnerStudentId);
        await AddStudentInDbAsync(otherReviewerStudentId);
        await AddGroupStudentInDbAsync(groupId, reviewerStudentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Reviewing);
        var homeworkName = await GetHomeworkNameInDbAsync(homeworkId);
        var firstSubmittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, firstOwnerStudentId);
        var secondSubmittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, secondOwnerStudentId);
        await AddDistributionReviewerInDbAsync(firstSubmittedHomeworkId, reviewerStudentId);
        await AddDistributionReviewerInDbAsync(secondSubmittedHomeworkId, reviewerStudentId);
        await AddDistributionReviewerInDbAsync(secondSubmittedHomeworkId, otherReviewerStudentId);
        await AddSubmittedReviewInDbAsync(secondSubmittedHomeworkId, reviewerStudentId);

        var request = _fixture.Build<V1ListAssignedReviewsRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();

        // Act
        var response = await ListAssignedReviewsClient.V1ListAssignedReviewsAsync(request);

        // Assert
        response.AssignedReviews.Should().BeEquivalentTo(
        [
            new
            {
                SubmittedHomeworkId = firstSubmittedHomeworkId,
                HomeworkName = homeworkName,
                IsReviewed = false
            },
            new
            {
                SubmittedHomeworkId = secondSubmittedHomeworkId,
                HomeworkName = homeworkName,
                IsReviewed = true
            }
        ]);
    }

    [Fact]
    public async Task V1ListAssignedReviews_GroupStudentHasAssignedReviews_ShouldReturnAssignedReviews()
    {
        // Arrange
        var teacherId = _fixture.Create<int>();
        var reviewerStudentId = teacherId + 1;
        var ownerStudentId = teacherId + 2;
        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(reviewerStudentId);
        await AddStudentInDbAsync(ownerStudentId);
        await AddGroupStudentInDbAsync(groupId, reviewerStudentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Reviewing, groupId: groupId);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId);
        await AddDistributionReviewerInDbAsync(submittedHomeworkId, reviewerStudentId);

        var request = _fixture.Build<V1ListAssignedReviewsRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();

        // Act
        var response = await ListAssignedReviewsClient.V1ListAssignedReviewsAsync(request);

        // Assert
        response.AssignedReviews.Should().ContainSingle().Which.SubmittedHomeworkId.Should().Be(submittedHomeworkId);
    }

    [Fact]
    public async Task V1ListAssignedReviews_NoAssignedReviews_ShouldReturnEmptyAssignedReviews()
    {
        // Arrange
        var teacherId = _fixture.Create<int>();
        var reviewerStudentId = teacherId + 1;
        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(reviewerStudentId);
        await AddGroupStudentInDbAsync(groupId, reviewerStudentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Reviewing);
        var request = _fixture.Build<V1ListAssignedReviewsRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();

        // Act
        var response = await ListAssignedReviewsClient.V1ListAssignedReviewsAsync(request);

        // Assert
        response.AssignedReviews.Should().BeEmpty();
    }

    [Fact]
    public async Task V1ListAssignedReviews_HomeworkNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var request = _fixture.Build<V1ListAssignedReviewsRequest>()
            .With(result => result.HomeworkId, _fixture.Create<int>())
            .With(result => result.StudentId, _fixture.Create<int>())
            .Create();

        // Act
        var action = async () => await ListAssignedReviewsClient.V1ListAssignedReviewsAsync(request);

        // Assert
        var exception = await action.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task V1ListAssignedReviews_StudentHasNoAccess_ShouldReturnNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<int>();
        var reviewerStudentId = teacherId + 1;
        var courseId = await AddCourseInDbAsync();
        var otherCourseId = await AddCourseInDbAsync();
        var otherGroupId = await AddGroupInDbAsync(otherCourseId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(reviewerStudentId);
        await AddGroupStudentInDbAsync(otherGroupId, reviewerStudentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Reviewing);
        var request = _fixture.Build<V1ListAssignedReviewsRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();

        // Act
        var action = async () => await ListAssignedReviewsClient.V1ListAssignedReviewsAsync(request);

        // Assert
        var exception = await action.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task V1ListAssignedReviews_HomeworkNotReviewing_ShouldReturnNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<int>();
        var reviewerStudentId = teacherId + 1;
        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(reviewerStudentId);
        await AddGroupStudentInDbAsync(groupId, reviewerStudentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published);
        var request = _fixture.Build<V1ListAssignedReviewsRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();

        // Act
        var action = async () => await ListAssignedReviewsClient.V1ListAssignedReviewsAsync(request);

        // Assert
        var exception = await action.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1ListAssignedReviews_NotPositiveHomeworkId_ShouldReturnInvalidArgument(long homeworkId)
    {
        // Arrange
        var request = _fixture.Build<V1ListAssignedReviewsRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, _fixture.Create<int>())
            .Create();

        // Act
        var action = async () => await ListAssignedReviewsClient.V1ListAssignedReviewsAsync(request);

        // Assert
        var exception = await action.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.HomeworkId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1ListAssignedReviews_NotPositiveStudentId_ShouldReturnInvalidArgument(long studentId)
    {
        // Arrange
        var request = _fixture.Build<V1ListAssignedReviewsRequest>()
            .With(result => result.HomeworkId, _fixture.Create<int>())
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var action = async () => await ListAssignedReviewsClient.V1ListAssignedReviewsAsync(request);

        // Assert
        var exception = await action.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StudentId));
    }
}
