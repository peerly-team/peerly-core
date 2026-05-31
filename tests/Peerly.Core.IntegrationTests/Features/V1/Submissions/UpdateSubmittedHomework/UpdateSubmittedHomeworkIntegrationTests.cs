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

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.UpdateSubmittedHomework;

public sealed class UpdateSubmittedHomeworkIntegrationTests : UpdateSubmittedHomeworkIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public UpdateSubmittedHomeworkIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1UpdateSubmittedHomework_CourseHomeworkAndOwnerStudent_ShouldUpdateOnlyTargetSubmittedHomework()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var otherStudentId = studentId + 1;
        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);
        await AddStudentInDbAsync(otherStudentId);
        await AddGroupStudentInDbAsync(groupId, studentId);
        await AddGroupStudentInDbAsync(groupId, otherStudentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId, "Old comment");
        var otherSubmittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, otherStudentId, "Other comment");
        var request = CreateValidRequest(submittedHomeworkId, studentId, "Updated comment");

        // Act
        var response = await UpdateSubmittedHomeworkClient.V1UpdateSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateSubmittedHomeworkResponse.ResponseOneofCase.SuccessResponse);

        var targetComment = await GetSubmittedHomeworkCommentAsync(submittedHomeworkId);
        targetComment.Should().Be(request.Comment);
        var otherComment = await GetSubmittedHomeworkCommentAsync(otherSubmittedHomeworkId);
        otherComment.Should().Be("Other comment");
    }

    [Fact]
    public async Task V1UpdateSubmittedHomework_GroupHomeworkAndOwnerStudent_ShouldUpdateComment()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);
        await AddGroupStudentInDbAsync(groupId, studentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published, groupId: groupId);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId, "Old comment");
        var request = CreateValidRequest(submittedHomeworkId, studentId, "Updated group comment");

        // Act
        var response = await UpdateSubmittedHomeworkClient.V1UpdateSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateSubmittedHomeworkResponse.ResponseOneofCase.SuccessResponse);

        var comment = await GetSubmittedHomeworkCommentAsync(submittedHomeworkId);
        comment.Should().Be(request.Comment);
    }

    [Fact]
    public async Task V1UpdateSubmittedHomework_SubmittedHomeworkNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var request = CreateValidRequest(_fixture.Create<long>(), _fixture.Create<long>(), "Updated comment");

        // Act
        var response = await UpdateSubmittedHomeworkClient.V1UpdateSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateSubmittedHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Отправленный ответ к домашнему заданию не найден");
    }

    [Fact]
    public async Task V1UpdateSubmittedHomework_SubmittedHomeworkBelongsToAnotherStudent_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var ownerStudentId = _fixture.Create<long>();
        var requestingStudentId = ownerStudentId + 1;
        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(requestingStudentId);
        await AddGroupStudentInDbAsync(groupId, ownerStudentId);
        await AddGroupStudentInDbAsync(groupId, requestingStudentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId, "Old comment");
        var request = CreateValidRequest(submittedHomeworkId, requestingStudentId, "Updated comment");

        // Act
        var response = await UpdateSubmittedHomeworkClient.V1UpdateSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateSubmittedHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Отправленный ответ к домашнему заданию не найден");

        var comment = await GetSubmittedHomeworkCommentAsync(submittedHomeworkId);
        comment.Should().Be("Old comment");
    }

    [Fact]
    public async Task V1UpdateSubmittedHomework_LinkedHomeworkMissing_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var studentId = _fixture.Create<long>();
        var missingHomeworkId = _fixture.Create<long>();

        await AddStudentInDbAsync(studentId);

        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(missingHomeworkId, studentId, "Old comment");
        var request = CreateValidRequest(submittedHomeworkId, studentId, "Updated comment");

        // Act
        var response = await UpdateSubmittedHomeworkClient.V1UpdateSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateSubmittedHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Домашнее задание не найдено");

        var comment = await GetSubmittedHomeworkCommentAsync(submittedHomeworkId);
        comment.Should().Be("Old comment");
    }

    [Fact]
    public async Task V1UpdateSubmittedHomework_HomeworkDraft_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);
        await AddGroupStudentInDbAsync(groupId, studentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Draft);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId, "Old comment");
        var request = CreateValidRequest(submittedHomeworkId, studentId, "Updated comment");

        // Act
        var response = await UpdateSubmittedHomeworkClient.V1UpdateSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateSubmittedHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Домашнее задание не найдено");

        var comment = await GetSubmittedHomeworkCommentAsync(submittedHomeworkId);
        comment.Should().Be("Old comment");
    }

    [Theory]
    [InlineData(HomeworkStatusModel.Reviewing)]
    [InlineData(HomeworkStatusModel.Confirmation)]
    [InlineData(HomeworkStatusModel.Finished)]
    public async Task V1UpdateSubmittedHomework_HomeworkNotPublished_ShouldBeOtherErrorConflict(HomeworkStatusModel status)
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);
        await AddGroupStudentInDbAsync(groupId, studentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, status);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId, "Old comment");
        var request = CreateValidRequest(submittedHomeworkId, studentId, "Updated comment");

        // Act
        var response = await UpdateSubmittedHomeworkClient.V1UpdateSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateSubmittedHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.Conflict);
        response.OtherError.Message.Should().Be("Отправка ответов для домашнего задания закрыта");

        var comment = await GetSubmittedHomeworkCommentAsync(submittedHomeworkId);
        comment.Should().Be("Old comment");
    }

    [Fact]
    public async Task V1UpdateSubmittedHomework_DeadlinePassed_ShouldBeOtherErrorConflict()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);
        await AddGroupStudentInDbAsync(groupId, studentId);

        var homeworkId = await AddHomeworkInDbAsync(
            courseId,
            teacherId,
            HomeworkStatusModel.Published,
            deadline: DateTimeOffset.UtcNow.AddDays(-1));
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId, "Old comment");
        var request = CreateValidRequest(submittedHomeworkId, studentId, "Updated comment");

        // Act
        var response = await UpdateSubmittedHomeworkClient.V1UpdateSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateSubmittedHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.Conflict);
        response.OtherError.Message.Should().Be("Отправка ответов для домашнего задания закрыта");

        var comment = await GetSubmittedHomeworkCommentAsync(submittedHomeworkId);
        comment.Should().Be("Old comment");
    }

    [Fact]
    public async Task V1UpdateSubmittedHomework_CourseStudentNotInCourse_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId, "Old comment");
        var request = CreateValidRequest(submittedHomeworkId, studentId, "Updated comment");

        // Act
        var response = await UpdateSubmittedHomeworkClient.V1UpdateSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateSubmittedHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Домашнее задание не найдено");

        var comment = await GetSubmittedHomeworkCommentAsync(submittedHomeworkId);
        comment.Should().Be("Old comment");
    }

    [Fact]
    public async Task V1UpdateSubmittedHomework_GroupStudentNotInGroup_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var targetGroupId = await AddGroupInDbAsync(courseId);
        var otherGroupId = await AddGroupInDbAsync(courseId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);
        await AddGroupStudentInDbAsync(otherGroupId, studentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published, groupId: targetGroupId);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId, "Old comment");
        var request = CreateValidRequest(submittedHomeworkId, studentId, "Updated comment");

        // Act
        var response = await UpdateSubmittedHomeworkClient.V1UpdateSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateSubmittedHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Домашнее задание не найдено");

        var comment = await GetSubmittedHomeworkCommentAsync(submittedHomeworkId);
        comment.Should().Be("Old comment");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1UpdateSubmittedHomework_NotPositiveSubmittedHomeworkId_ShouldReturnInvalidArgument(long submittedHomeworkId)
    {
        // Arrange
        var request = CreateValidRequest(submittedHomeworkId, 1, "Updated comment");

        // Act
        var act = async () => await UpdateSubmittedHomeworkClient.V1UpdateSubmittedHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.SubmittedHomeworkId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1UpdateSubmittedHomework_NotPositiveStudentId_ShouldReturnInvalidArgument(long studentId)
    {
        // Arrange
        var request = CreateValidRequest(1, studentId, "Updated comment");

        // Act
        var act = async () => await UpdateSubmittedHomeworkClient.V1UpdateSubmittedHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StudentId));
    }

    private static V1UpdateSubmittedHomeworkRequest CreateValidRequest(long submittedHomeworkId, long studentId, string? comment)
    {
        return new V1UpdateSubmittedHomeworkRequest
        {
            SubmittedHomeworkId = submittedHomeworkId,
            StudentId = studentId,
            Comment = comment
        };
    }

    private async Task<string?> GetSubmittedHomeworkCommentAsync(long submittedHomeworkId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.QuerySingleAsync<string?>(
            "select comment from submitted_homeworks where id = @submittedHomeworkId",
            new { submittedHomeworkId });
    }
}
