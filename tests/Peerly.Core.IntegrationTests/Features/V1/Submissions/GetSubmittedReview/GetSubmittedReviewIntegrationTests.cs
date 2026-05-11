using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using HomeworkStatusModel = Peerly.Core.Models.Homeworks.HomeworkStatus;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.GetSubmittedReview;

public sealed class GetSubmittedReviewIntegrationTests : GetSubmittedReviewIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public GetSubmittedReviewIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1GetSubmittedReview_SubmittedReviewBelongsToStudent_ShouldReturnSubmittedReview()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var ownerStudentId = _fixture.Create<long>();
        var reviewerStudentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(reviewerStudentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Confirmation);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId);
        var submittedReviewId = await AddSubmittedReviewInDbAsync(submittedHomeworkId, reviewerStudentId, mark: 81, comment: "Review comment");
        var request = _fixture.Build<V1GetSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, submittedReviewId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();

        // Act
        var response = await GetSubmittedReviewClient.V1GetSubmittedReviewAsync(request);

        // Assert
        response.SubmittedReview.Should().BeEquivalentTo(new
        {
            Id = submittedReviewId,
            Mark = 81,
            Comment = "Review comment"
        });
    }

    [Fact]
    public async Task V1GetSubmittedReview_SubmittedReviewNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var request = _fixture.Build<V1GetSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, _fixture.Create<long>())
            .With(result => result.StudentId, _fixture.Create<long>())
            .Create();

        // Act
        var act = async () => await GetSubmittedReviewClient.V1GetSubmittedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task V1GetSubmittedReview_WrongStudent_ShouldReturnNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var ownerStudentId = _fixture.Create<long>();
        var reviewerStudentId = _fixture.Create<long>();
        var requestingStudentId = reviewerStudentId + 1;
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(reviewerStudentId);
        await AddStudentInDbAsync(requestingStudentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Confirmation);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId);
        var submittedReviewId = await AddSubmittedReviewInDbAsync(submittedHomeworkId, reviewerStudentId);
        var request = _fixture.Build<V1GetSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, submittedReviewId)
            .With(result => result.StudentId, requestingStudentId)
            .Create();

        // Act
        var act = async () => await GetSubmittedReviewClient.V1GetSubmittedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetSubmittedReview_NotPositiveSubmittedReviewId_ShouldReturnInvalidArgument(long submittedReviewId)
    {
        // Arrange
        var request = _fixture.Build<V1GetSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, submittedReviewId)
            .With(result => result.StudentId, 1)
            .Create();

        // Act
        var act = async () => await GetSubmittedReviewClient.V1GetSubmittedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.SubmittedReviewId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetSubmittedReview_NotPositiveStudentId_ShouldReturnInvalidArgument(long studentId)
    {
        // Arrange
        var request = _fixture.Build<V1GetSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, 1)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var act = async () => await GetSubmittedReviewClient.V1GetSubmittedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StudentId));
    }
}
