using System;
using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using HomeworkStatusModel = Peerly.Core.Models.Homeworks.HomeworkStatus;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.DeleteSubmittedReview;

public sealed class DeleteSubmittedReviewIntegrationTests : DeleteSubmittedReviewIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public DeleteSubmittedReviewIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1DeleteSubmittedReview_ReviewingHomeworkAndAuthorStudent_ShouldDeleteOnlyTargetReview()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var ownerStudentId = _fixture.Create<long>();
        var reviewerStudentId = _fixture.Create<long>();
        var otherReviewerStudentId = reviewerStudentId + 1;
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(reviewerStudentId);
        await AddStudentInDbAsync(otherReviewerStudentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Reviewing);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId);
        var submittedReviewId = await AddSubmittedReviewInDbAsync(submittedHomeworkId, reviewerStudentId);
        var otherSubmittedReviewId = await AddSubmittedReviewInDbAsync(submittedHomeworkId, otherReviewerStudentId);
        var request = _fixture.Build<V1DeleteSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, submittedReviewId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();

        // Act
        var response = await DeleteSubmittedReviewClient.V1DeleteSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteSubmittedReviewResponse.ResponseOneofCase.SuccessResponse);

        var targetSubmittedReviewsCount = await GetSubmittedReviewsCountAsync(submittedReviewId);
        targetSubmittedReviewsCount.Should().Be(0);
        var otherSubmittedReviewsCount = await GetSubmittedReviewsCountAsync(otherSubmittedReviewId);
        otherSubmittedReviewsCount.Should().Be(1);
    }

    [Fact]
    public async Task V1DeleteSubmittedReview_SubmittedReviewNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var request = _fixture.Build<V1DeleteSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, _fixture.Create<long>())
            .With(result => result.StudentId, _fixture.Create<long>())
            .Create();

        // Act
        var response = await DeleteSubmittedReviewClient.V1DeleteSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteSubmittedReviewResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Отправленная рецензия не найдена");
    }

    [Fact]
    public async Task V1DeleteSubmittedReview_SubmittedReviewBelongsToAnotherStudent_ShouldBeOtherErrorPermissionDenied()
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

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Reviewing);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId);
        var submittedReviewId = await AddSubmittedReviewInDbAsync(submittedHomeworkId, reviewerStudentId);
        var request = _fixture.Build<V1DeleteSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, submittedReviewId)
            .With(result => result.StudentId, requestingStudentId)
            .Create();

        // Act
        var response = await DeleteSubmittedReviewClient.V1DeleteSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteSubmittedReviewResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.PermissionDenied);

        var submittedReviewsCount = await GetSubmittedReviewsCountAsync(submittedReviewId);
        submittedReviewsCount.Should().Be(1);
    }

    [Fact]
    public async Task V1DeleteSubmittedReview_LinkedSubmittedHomeworkMissing_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var reviewerStudentId = _fixture.Create<long>();
        var missingSubmittedHomeworkId = _fixture.Create<long>();

        await AddStudentInDbAsync(reviewerStudentId);

        var submittedReviewId = await AddSubmittedReviewInDbAsync(missingSubmittedHomeworkId, reviewerStudentId);
        var request = _fixture.Build<V1DeleteSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, submittedReviewId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();

        // Act
        var response = await DeleteSubmittedReviewClient.V1DeleteSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteSubmittedReviewResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Отправленный ответ к домашнему заданию не найден");

        var submittedReviewsCount = await GetSubmittedReviewsCountAsync(submittedReviewId);
        submittedReviewsCount.Should().Be(1);
    }

    [Fact]
    public async Task V1DeleteSubmittedReview_LinkedHomeworkMissing_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var ownerStudentId = _fixture.Create<long>();
        var reviewerStudentId = _fixture.Create<long>();
        var missingHomeworkId = _fixture.Create<long>();

        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(reviewerStudentId);

        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(missingHomeworkId, ownerStudentId);
        var submittedReviewId = await AddSubmittedReviewInDbAsync(submittedHomeworkId, reviewerStudentId);
        var request = _fixture.Build<V1DeleteSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, submittedReviewId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();

        // Act
        var response = await DeleteSubmittedReviewClient.V1DeleteSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteSubmittedReviewResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Домашнее задание не найдено");

        var submittedReviewsCount = await GetSubmittedReviewsCountAsync(submittedReviewId);
        submittedReviewsCount.Should().Be(1);
    }

    [Theory]
    [InlineData(HomeworkStatusModel.Draft)]
    [InlineData(HomeworkStatusModel.Published)]
    [InlineData(HomeworkStatusModel.Confirmation)]
    [InlineData(HomeworkStatusModel.Finished)]
    public async Task V1DeleteSubmittedReview_HomeworkNotInReviewingStatus_ShouldBeValidationError(HomeworkStatusModel status)
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
        var submittedReviewId = await AddSubmittedReviewInDbAsync(submittedHomeworkId, reviewerStudentId);
        var request = _fixture.Build<V1DeleteSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, submittedReviewId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();

        // Act
        var response = await DeleteSubmittedReviewClient.V1DeleteSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteSubmittedReviewResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainSingle("Проверка домашнего задания закрыта");

        var submittedReviewsCount = await GetSubmittedReviewsCountAsync(submittedReviewId);
        submittedReviewsCount.Should().Be(1);
    }

    [Fact]
    public async Task V1DeleteSubmittedReview_ReviewDeadlinePassed_ShouldBeValidationError()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var ownerStudentId = _fixture.Create<long>();
        var reviewerStudentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(reviewerStudentId);

        var homeworkId = await AddHomeworkInDbAsync(
            courseId,
            teacherId,
            HomeworkStatusModel.Reviewing,
            DateTimeOffset.UtcNow.AddDays(-1));
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId);
        var submittedReviewId = await AddSubmittedReviewInDbAsync(submittedHomeworkId, reviewerStudentId);
        var request = _fixture.Build<V1DeleteSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, submittedReviewId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();

        // Act
        var response = await DeleteSubmittedReviewClient.V1DeleteSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteSubmittedReviewResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainSingle("Проверка домашнего задания закрыта");

        var submittedReviewsCount = await GetSubmittedReviewsCountAsync(submittedReviewId);
        submittedReviewsCount.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1DeleteSubmittedReview_NotPositiveSubmittedReviewId_ShouldReturnInvalidArgument(long submittedReviewId)
    {
        // Arrange
        var request = _fixture.Build<V1DeleteSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, submittedReviewId)
            .With(result => result.StudentId, 1)
            .Create();

        // Act
        var act = async () => await DeleteSubmittedReviewClient.V1DeleteSubmittedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.SubmittedReviewId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1DeleteSubmittedReview_NotPositiveStudentId_ShouldReturnInvalidArgument(long studentId)
    {
        // Arrange
        var request = _fixture.Build<V1DeleteSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, 1)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var act = async () => await DeleteSubmittedReviewClient.V1DeleteSubmittedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StudentId));
    }

    private async Task<int> GetSubmittedReviewsCountAsync(long submittedReviewId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.QuerySingleAsync<int>(
            "select count(*) from submitted_reviews where id = @submittedReviewId",
            new { submittedReviewId });
    }
}
