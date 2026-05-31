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

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.UpdateSubmittedReview;

public sealed class UpdateSubmittedReviewIntegrationTests : UpdateSubmittedReviewIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public UpdateSubmittedReviewIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1UpdateSubmittedReview_ReviewingHomeworkAndAuthorStudent_ShouldUpdateOnlyTargetReview()
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

        var rubricId = await AddRubricInDbAsync(teacherId);
        var criterionId = await AddRubricCriterionInDbAsync(rubricId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Reviewing, rubricId: rubricId);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId);
        var submittedReviewId = await AddSubmittedReviewInDbAsync(submittedHomeworkId, reviewerStudentId, mark: 50, comment: "Old comment");
        var otherSubmittedReviewId = await AddSubmittedReviewInDbAsync(submittedHomeworkId, otherReviewerStudentId, mark: 70, comment: "Other comment");
        var request = _fixture.Build<V1UpdateSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, submittedReviewId)
            .With(result => result.StudentId, reviewerStudentId)
            .With(result => result.Comment, "Updated comment")
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = criterionId, Score = 95 });

        // Act
        var response = await UpdateSubmittedReviewClient.V1UpdateSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateSubmittedReviewResponse.ResponseOneofCase.SuccessResponse);

        var targetSubmittedReview = await GetSubmittedReviewAsync(submittedReviewId);
        targetSubmittedReview.Comment.Should().Be(request.Comment);

        var otherSubmittedReview = await GetSubmittedReviewAsync(otherSubmittedReviewId);
        otherSubmittedReview.Comment.Should().Be("Other comment");
    }

    [Fact]
    public async Task V1UpdateSubmittedReview_SubmittedReviewNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var request = _fixture.Build<V1UpdateSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, _fixture.Create<long>())
            .With(result => result.StudentId, _fixture.Create<long>())
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = 95 });

        // Act
        var response = await UpdateSubmittedReviewClient.V1UpdateSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateSubmittedReviewResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Отправленная рецензия не найдена");
    }

    [Fact]
    public async Task V1UpdateSubmittedReview_SubmittedReviewBelongsToAnotherStudent_ShouldBeOtherErrorPermissionDenied()
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
        var submittedReviewId = await AddSubmittedReviewInDbAsync(submittedHomeworkId, reviewerStudentId, mark: 50, comment: "Old comment");
        var request = _fixture.Build<V1UpdateSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, submittedReviewId)
            .With(result => result.StudentId, requestingStudentId)
            .With(result => result.Comment, "Updated comment")
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = 95 });

        // Act
        var response = await UpdateSubmittedReviewClient.V1UpdateSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateSubmittedReviewResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.PermissionDenied);

        var submittedReview = await GetSubmittedReviewAsync(submittedReviewId);
        submittedReview.Comment.Should().Be("Old comment");
    }

    [Fact]
    public async Task V1UpdateSubmittedReview_LinkedSubmittedHomeworkMissing_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var reviewerStudentId = _fixture.Create<long>();
        var missingSubmittedHomeworkId = _fixture.Create<long>();

        await AddStudentInDbAsync(reviewerStudentId);

        var submittedReviewId = await AddSubmittedReviewInDbAsync(missingSubmittedHomeworkId, reviewerStudentId, mark: 50, comment: "Old comment");
        var request = _fixture.Build<V1UpdateSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, submittedReviewId)
            .With(result => result.StudentId, reviewerStudentId)
            .With(result => result.Comment, "Updated comment")
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = 95 });

        // Act
        var response = await UpdateSubmittedReviewClient.V1UpdateSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateSubmittedReviewResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Отправленный ответ к домашнему заданию не найден");

        var submittedReview = await GetSubmittedReviewAsync(submittedReviewId);
        submittedReview.Comment.Should().Be("Old comment");
    }

    [Fact]
    public async Task V1UpdateSubmittedReview_LinkedHomeworkMissing_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var ownerStudentId = _fixture.Create<long>();
        var reviewerStudentId = _fixture.Create<long>();
        var missingHomeworkId = _fixture.Create<long>();

        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(reviewerStudentId);

        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(missingHomeworkId, ownerStudentId);
        var submittedReviewId = await AddSubmittedReviewInDbAsync(submittedHomeworkId, reviewerStudentId, mark: 50, comment: "Old comment");
        var request = _fixture.Build<V1UpdateSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, submittedReviewId)
            .With(result => result.StudentId, reviewerStudentId)
            .With(result => result.Comment, "Updated comment")
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = 95 });

        // Act
        var response = await UpdateSubmittedReviewClient.V1UpdateSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateSubmittedReviewResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Домашнее задание не найдено");

        var submittedReview = await GetSubmittedReviewAsync(submittedReviewId);
        submittedReview.Comment.Should().Be("Old comment");
    }

    [Theory]
    [InlineData(HomeworkStatusModel.Draft)]
    [InlineData(HomeworkStatusModel.Published)]
    [InlineData(HomeworkStatusModel.Confirmation)]
    [InlineData(HomeworkStatusModel.Finished)]
    public async Task V1UpdateSubmittedReview_HomeworkNotInReviewingStatus_ShouldBeValidationError(HomeworkStatusModel status)
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
        var submittedReviewId = await AddSubmittedReviewInDbAsync(submittedHomeworkId, reviewerStudentId, mark: 50, comment: "Old comment");
        var request = _fixture.Build<V1UpdateSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, submittedReviewId)
            .With(result => result.StudentId, reviewerStudentId)
            .With(result => result.Comment, "Updated comment")
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = 95 });

        // Act
        var response = await UpdateSubmittedReviewClient.V1UpdateSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateSubmittedReviewResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainSingle("Проверка домашнего задания закрыта");

        var submittedReview = await GetSubmittedReviewAsync(submittedReviewId);
        submittedReview.Comment.Should().Be("Old comment");
    }

    [Fact]
    public async Task V1UpdateSubmittedReview_ReviewDeadlinePassed_ShouldBeValidationError()
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
        var submittedReviewId = await AddSubmittedReviewInDbAsync(submittedHomeworkId, reviewerStudentId, mark: 50, comment: "Old comment");
        var request = _fixture.Build<V1UpdateSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, submittedReviewId)
            .With(result => result.StudentId, reviewerStudentId)
            .With(result => result.Comment, "Updated comment")
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = 95 });

        // Act
        var response = await UpdateSubmittedReviewClient.V1UpdateSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateSubmittedReviewResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainSingle("Проверка домашнего задания закрыта");

        var submittedReview = await GetSubmittedReviewAsync(submittedReviewId);
        submittedReview.Comment.Should().Be("Old comment");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1UpdateSubmittedReview_NotPositiveSubmittedReviewId_ShouldReturnInvalidArgument(long submittedReviewId)
    {
        // Arrange
        var request = _fixture.Build<V1UpdateSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, submittedReviewId)
            .With(result => result.StudentId, 1)
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = 95 });

        // Act
        var act = async () => await UpdateSubmittedReviewClient.V1UpdateSubmittedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.SubmittedReviewId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1UpdateSubmittedReview_NotPositiveStudentId_ShouldReturnInvalidArgument(long studentId)
    {
        // Arrange
        var request = _fixture.Build<V1UpdateSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, 1)
            .With(result => result.StudentId, studentId)
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = 95 });

        // Act
        var act = async () => await UpdateSubmittedReviewClient.V1UpdateSubmittedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StudentId));
    }

    [Fact]
    public async Task V1UpdateSubmittedReview_ScoreOutOfRange_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1UpdateSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, 1)
            .With(result => result.StudentId, 1)
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = -1 });

        // Act
        var act = async () => await UpdateSubmittedReviewClient.V1UpdateSubmittedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain("Score");
    }

    [Fact]
    public async Task V1UpdateSubmittedReview_EmptyComment_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1UpdateSubmittedReviewRequest>()
            .With(result => result.SubmittedReviewId, 1)
            .With(result => result.StudentId, 1)
            .With(result => result.Comment, string.Empty)
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = 95 });

        // Act
        var act = async () => await UpdateSubmittedReviewClient.V1UpdateSubmittedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Comment));
    }

    private async Task<(string Comment, long Id)> GetSubmittedReviewAsync(long submittedReviewId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        var row = await connection.QuerySingleAsync(
            "select comment, id from submitted_reviews where id = @submittedReviewId",
            new { submittedReviewId });
        return (row.comment, row.id);
    }
}
