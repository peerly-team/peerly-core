using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;
using HomeworkStatusModel = Peerly.Core.Models.Homeworks.HomeworkStatus;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.GetAssignedReview;

public sealed class GetAssignedReviewIntegrationTests : GetAssignedReviewIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public GetAssignedReviewIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1GetAssignedReview_ReviewingHomeworkAndAssignedReviewer_ShouldReturnSubmissionForReview()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var ownerStudentId = _fixture.Create<long>();
        var reviewerStudentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(reviewerStudentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Reviewing);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId);
        await AddDistributionReviewerInDbAsync(submittedHomeworkId, reviewerStudentId);
        var submittedReviewId = await AddSubmittedReviewInDbAsync(submittedHomeworkId, reviewerStudentId);

        var originalFile = await AddFileInDbAsync(_fixture.Create<string>(), 1024);
        var anonymizedFile = await AddFileInDbAsync(_fixture.Create<string>(), 2048);
        await AddSubmittedHomeworkFileInDbAsync(submittedHomeworkId, originalFile.Id, anonymizedFile.Id);

        var request = _fixture.Build<V1GetAssignedReviewRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();

        // Act
        var response = await GetAssignedReviewClient.V1GetAssignedReviewAsync(request);

        // Assert
        response.Submission.SubmittedHomeworkId.Should().Be(submittedHomeworkId);
        response.Submission.Comment.Should().Be("Test comment");
        response.Submission.Checklist.Should().Be("Checklist");
        response.Submission.SubmittedReviewId.Should().Be(submittedReviewId);
        response.Submission.Files.Should().ContainSingle().Which
            .Should().BeEquivalentTo(new { anonymizedFile.Id, anonymizedFile.Name, anonymizedFile.Size });
    }

    [Fact]
    public async Task V1GetAssignedReview_SubmittedHomeworkNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var request = _fixture.Build<V1GetAssignedReviewRequest>()
            .With(result => result.SubmittedHomeworkId, _fixture.Create<long>())
            .With(result => result.StudentId, _fixture.Create<long>())
            .Create();

        // Act
        var act = async () => await GetAssignedReviewClient.V1GetAssignedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task V1GetAssignedReview_ReviewerIsNotAssigned_ShouldReturnNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var ownerStudentId = _fixture.Create<long>();
        var reviewerStudentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(reviewerStudentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Reviewing);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId);
        var request = _fixture.Build<V1GetAssignedReviewRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();

        // Act
        var act = async () => await GetAssignedReviewClient.V1GetAssignedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData(HomeworkStatusModel.Draft)]
    [InlineData(HomeworkStatusModel.Published)]
    [InlineData(HomeworkStatusModel.Confirmation)]
    [InlineData(HomeworkStatusModel.Finished)]
    public async Task V1GetAssignedReview_HomeworkNotInReviewingStatus_ShouldReturnFailedPrecondition(HomeworkStatusModel status)
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var ownerStudentId = _fixture.Create<long>();
        var reviewerStudentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(reviewerStudentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, status);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId);
        await AddDistributionReviewerInDbAsync(submittedHomeworkId, reviewerStudentId);
        var request = _fixture.Build<V1GetAssignedReviewRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();

        // Act
        var act = async () => await GetAssignedReviewClient.V1GetAssignedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.FailedPrecondition);
        exception.Which.Status.Detail.Should().Be("Проверка домашнего задания закрыта");
    }

    [Fact]
    public async Task V1GetAssignedReview_ReviewDeadlinePassed_ShouldReturnFailedPrecondition()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var ownerStudentId = _fixture.Create<long>();
        var reviewerStudentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(reviewerStudentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Reviewing, DateTimeOffset.UtcNow.AddDays(-1));
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId);
        await AddDistributionReviewerInDbAsync(submittedHomeworkId, reviewerStudentId);
        var request = _fixture.Build<V1GetAssignedReviewRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();

        // Act
        var act = async () => await GetAssignedReviewClient.V1GetAssignedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.FailedPrecondition);
        exception.Which.Status.Detail.Should().Be("Проверка домашнего задания закрыта");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetAssignedReview_NotPositiveSubmittedHomeworkId_ShouldReturnInvalidArgument(long submittedHomeworkId)
    {
        // Arrange
        var request = _fixture.Build<V1GetAssignedReviewRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, 1)
            .Create();

        // Act
        var act = async () => await GetAssignedReviewClient.V1GetAssignedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.SubmittedHomeworkId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetAssignedReview_NotPositiveStudentId_ShouldReturnInvalidArgument(long studentId)
    {
        // Arrange
        var request = _fixture.Build<V1GetAssignedReviewRequest>()
            .With(result => result.SubmittedHomeworkId, 1)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var act = async () => await GetAssignedReviewClient.V1GetAssignedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StudentId));
    }
}
