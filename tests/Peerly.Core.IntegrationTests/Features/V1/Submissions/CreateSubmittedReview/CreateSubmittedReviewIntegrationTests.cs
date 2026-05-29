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

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.CreateSubmittedReview;

public sealed class CreateSubmittedReviewIntegrationTests : CreateSubmittedReviewIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public CreateSubmittedReviewIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1CreateSubmittedReview_ReviewingHomeworkAndAssignedReviewer_ShouldCreateSubmittedReview()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var ownerStudentId = _fixture.Create<long>();
        var reviewerStudentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(reviewerStudentId);

        var rubricId = await AddRubricInDbAsync(teacherId);
        var criterionId = await AddRubricCriterionInDbAsync(rubricId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Reviewing, rubricId: rubricId);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId);
        await AddDistributionReviewerInDbAsync(submittedHomeworkId, reviewerStudentId);

        var request = _fixture.Build<V1CreateSubmittedReviewRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = criterionId, Score = 95 });

        // Act
        var response = await CreateSubmittedReviewClient.V1CreateSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedReviewResponse.ResponseOneofCase.SuccessResponse);
        response.SuccessResponse.SubmittedReviewId.Should().BeGreaterThan(0);

        var submittedReview = await GetSubmittedReviewAsync(response.SuccessResponse.SubmittedReviewId);
        submittedReview.Should().BeEquivalentTo((request.SubmittedHomeworkId, request.StudentId, request.Comment));
    }

    [Fact]
    public async Task V1CreateSubmittedReview_SubmittedHomeworkNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var request = _fixture.Build<V1CreateSubmittedReviewRequest>()
            .With(result => result.SubmittedHomeworkId, _fixture.Create<long>())
            .With(result => result.StudentId, _fixture.Create<long>())
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = 95 });

        // Act
        var response = await CreateSubmittedReviewClient.V1CreateSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedReviewResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Отправленный ответ к домашнему заданию не найден");

        var submittedReviewsCount = await GetSubmittedReviewsCountAsync(request.SubmittedHomeworkId, request.StudentId);
        submittedReviewsCount.Should().Be(0);
    }

    [Fact]
    public async Task V1CreateSubmittedReview_ReviewerIsNotAssigned_ShouldBeOtherErrorPermissionDenied()
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
        var request = _fixture.Build<V1CreateSubmittedReviewRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = 95 });

        // Act
        var response = await CreateSubmittedReviewClient.V1CreateSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedReviewResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.PermissionDenied);

        var submittedReviewsCount = await GetSubmittedReviewsCountAsync(request.SubmittedHomeworkId, request.StudentId);
        submittedReviewsCount.Should().Be(0);
    }

    [Fact]
    public async Task V1CreateSubmittedReview_LinkedHomeworkMissing_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var ownerStudentId = _fixture.Create<long>();
        var reviewerStudentId = _fixture.Create<long>();
        var missingHomeworkId = _fixture.Create<long>();

        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(reviewerStudentId);

        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(missingHomeworkId, ownerStudentId);
        await AddDistributionReviewerInDbAsync(submittedHomeworkId, reviewerStudentId);
        var request = _fixture.Build<V1CreateSubmittedReviewRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = 95 });

        // Act
        var response = await CreateSubmittedReviewClient.V1CreateSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedReviewResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Домашнее задание не найдено");

        var submittedReviewsCount = await GetSubmittedReviewsCountAsync(request.SubmittedHomeworkId, request.StudentId);
        submittedReviewsCount.Should().Be(0);
    }

    [Theory]
    [InlineData(HomeworkStatusModel.Draft)]
    [InlineData(HomeworkStatusModel.Published)]
    [InlineData(HomeworkStatusModel.Confirmation)]
    [InlineData(HomeworkStatusModel.Finished)]
    public async Task V1CreateSubmittedReview_HomeworkNotInReviewingStatus_ShouldBeValidationError(HomeworkStatusModel status)
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
        var request = _fixture.Build<V1CreateSubmittedReviewRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = 95 });

        // Act
        var response = await CreateSubmittedReviewClient.V1CreateSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedReviewResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainSingle("Проверка домашнего задания закрыта");

        var submittedReviewsCount = await GetSubmittedReviewsCountAsync(request.SubmittedHomeworkId, request.StudentId);
        submittedReviewsCount.Should().Be(0);
    }

    [Fact]
    public async Task V1CreateSubmittedReview_ReviewDeadlinePassed_ShouldBeValidationError()
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
        var request = _fixture.Build<V1CreateSubmittedReviewRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = 95 });

        // Act
        var response = await CreateSubmittedReviewClient.V1CreateSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedReviewResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainSingle("Проверка домашнего задания закрыта");

        var submittedReviewsCount = await GetSubmittedReviewsCountAsync(request.SubmittedHomeworkId, request.StudentId);
        submittedReviewsCount.Should().Be(0);
    }

    [Fact]
    public async Task V1CreateSubmittedReview_SubmittedReviewAlreadyExists_ShouldBeOtherErrorConflict()
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
        await AddSubmittedReviewInDbAsync(submittedHomeworkId, reviewerStudentId);
        var request = _fixture.Build<V1CreateSubmittedReviewRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, reviewerStudentId)
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = 95 });

        // Act
        var response = await CreateSubmittedReviewClient.V1CreateSubmittedReviewAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedReviewResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.Conflict);

        var submittedReviewsCount = await GetSubmittedReviewsCountAsync(request.SubmittedHomeworkId, request.StudentId);
        submittedReviewsCount.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1CreateSubmittedReview_NotPositiveSubmittedHomeworkId_ShouldReturnInvalidArgument(long submittedHomeworkId)
    {
        // Arrange
        var request = _fixture.Build<V1CreateSubmittedReviewRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, 1)
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = 95 });

        // Act
        var act = async () => await CreateSubmittedReviewClient.V1CreateSubmittedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.SubmittedHomeworkId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1CreateSubmittedReview_NotPositiveStudentId_ShouldReturnInvalidArgument(long studentId)
    {
        // Arrange
        var request = _fixture.Build<V1CreateSubmittedReviewRequest>()
            .With(result => result.SubmittedHomeworkId, 1)
            .With(result => result.StudentId, studentId)
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = 95 });

        // Act
        var act = async () => await CreateSubmittedReviewClient.V1CreateSubmittedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StudentId));
    }

    [Fact]
    public async Task V1CreateSubmittedReview_ScoreOutOfRange_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1CreateSubmittedReviewRequest>()
            .With(result => result.SubmittedHomeworkId, 1)
            .With(result => result.StudentId, 1)
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = -1 });

        // Act
        var act = async () => await CreateSubmittedReviewClient.V1CreateSubmittedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain("Score");
    }

    [Fact]
    public async Task V1CreateSubmittedReview_EmptyComment_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1CreateSubmittedReviewRequest>()
            .With(result => result.SubmittedHomeworkId, 1)
            .With(result => result.StudentId, 1)
            .With(result => result.Comment, string.Empty)
            .Create();
        request.Scores.Add(new SubmittedReviewScoreInput { RubricCriterionId = 1, Score = 95 });

        // Act
        var act = async () => await CreateSubmittedReviewClient.V1CreateSubmittedReviewAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Comment));
    }

    private async Task<(long SubmittedHomeworkId, long StudentId, string Comment)> GetSubmittedReviewAsync(long submittedReviewId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        var row = await connection.QuerySingleAsync(
            "select submitted_homework_id, student_id, comment from submitted_reviews where id = @submittedReviewId",
            new { submittedReviewId });
        return (row.submitted_homework_id, row.student_id, row.comment);
    }

    private async Task<int> GetSubmittedReviewsCountAsync(long submittedHomeworkId, long studentId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.QuerySingleAsync<int>(
            "select count(*) from submitted_reviews where submitted_homework_id = @submittedHomeworkId and student_id = @studentId",
            new { submittedHomeworkId, studentId });
    }
}
