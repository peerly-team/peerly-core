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

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.CreateSubmittedHomework;

public sealed class CreateSubmittedHomeworkIntegrationTests : CreateSubmittedHomeworkIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public CreateSubmittedHomeworkIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1CreateSubmittedHomework_CourseHomeworkPublishedAndStudentInCourse_ShouldCreateSubmittedHomework()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);
        await AddGroupStudentInDbAsync(groupId, studentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published);
        var request = _fixture.Build<V1CreateSubmittedHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await CreateSubmittedHomeworkClient.V1CreateSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedHomeworkResponse.ResponseOneofCase.SuccessResponse);
        response.SuccessResponse.SubmittedHomeworkId.Should().BeGreaterThan(0);

        var submittedHomework = await GetSubmittedHomeworkAsync(response.SuccessResponse.SubmittedHomeworkId);
        submittedHomework.Should().BeEquivalentTo((request.HomeworkId, request.StudentId, request.Comment));
    }

    [Fact]
    public async Task V1CreateSubmittedHomework_GroupHomeworkPublishedAndStudentInGroup_ShouldCreateSubmittedHomework()
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
        var request = _fixture.Build<V1CreateSubmittedHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await CreateSubmittedHomeworkClient.V1CreateSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedHomeworkResponse.ResponseOneofCase.SuccessResponse);
        response.SuccessResponse.SubmittedHomeworkId.Should().BeGreaterThan(0);

        var submittedHomework = await GetSubmittedHomeworkAsync(response.SuccessResponse.SubmittedHomeworkId);
        submittedHomework.Should().BeEquivalentTo((request.HomeworkId, request.StudentId, request.Comment));
    }

    [Fact]
    public async Task V1CreateSubmittedHomework_HomeworkNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var request = _fixture.Create<V1CreateSubmittedHomeworkRequest>();

        // Act
        var response = await CreateSubmittedHomeworkClient.V1CreateSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Домашнее задание не найдено");

        var submittedHomeworksCount = await GetSubmittedHomeworksCountAsync(request.HomeworkId, request.StudentId);
        submittedHomeworksCount.Should().Be(0);
    }

    [Fact]
    public async Task V1CreateSubmittedHomework_StudentNotInCourse_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var otherStudentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);
        await AddStudentInDbAsync(otherStudentId);
        await AddGroupStudentInDbAsync(groupId, studentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published);
        var request = _fixture.Build<V1CreateSubmittedHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, otherStudentId)
            .Create();

        // Act
        var response = await CreateSubmittedHomeworkClient.V1CreateSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Домашнее задание не найдено");

        var submittedHomeworksCount = await GetSubmittedHomeworksCountAsync(request.HomeworkId, request.StudentId);
        submittedHomeworksCount.Should().Be(0);
    }

    [Fact]
    public async Task V1CreateSubmittedHomework_GroupHomeworkStudentNotInGroup_ShouldBeOtherErrorNotFound()
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
        var request = _fixture.Build<V1CreateSubmittedHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await CreateSubmittedHomeworkClient.V1CreateSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Домашнее задание не найдено");

        var submittedHomeworksCount = await GetSubmittedHomeworksCountAsync(request.HomeworkId, request.StudentId);
        submittedHomeworksCount.Should().Be(0);
    }

    [Fact]
    public async Task V1CreateSubmittedHomework_HomeworkInDraftStatus_ShouldBeOtherErrorNotFound()
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
        var request = _fixture.Build<V1CreateSubmittedHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await CreateSubmittedHomeworkClient.V1CreateSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Домашнее задание не найдено");

        var submittedHomeworksCount = await GetSubmittedHomeworksCountAsync(request.HomeworkId, request.StudentId);
        submittedHomeworksCount.Should().Be(0);
    }

    [Theory]
    [InlineData(HomeworkStatusModel.Reviewing)]
    [InlineData(HomeworkStatusModel.Confirmation)]
    [InlineData(HomeworkStatusModel.Finished)]
    public async Task V1CreateSubmittedHomework_HomeworkNotInPublishedStatus_ShouldBeOtherErrorConflict(HomeworkStatusModel status)
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
        var request = _fixture.Build<V1CreateSubmittedHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await CreateSubmittedHomeworkClient.V1CreateSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.Conflict);
        response.OtherError.Message.Should().Be("Отправка ответов для домашнего задания закрыта");

        var submittedHomeworksCount = await GetSubmittedHomeworksCountAsync(request.HomeworkId, request.StudentId);
        submittedHomeworksCount.Should().Be(0);
    }

    [Fact]
    public async Task V1CreateSubmittedHomework_DeadlinePassed_ShouldBeOtherErrorConflict()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);
        await AddGroupStudentInDbAsync(groupId, studentId);

        var pastDeadline = DateTimeOffset.UtcNow.AddDays(-1);
        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published, deadline: pastDeadline);
        var request = _fixture.Build<V1CreateSubmittedHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await CreateSubmittedHomeworkClient.V1CreateSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.Conflict);
        response.OtherError.Message.Should().Be("Отправка ответов для домашнего задания закрыта");

        var submittedHomeworksCount = await GetSubmittedHomeworksCountAsync(request.HomeworkId, request.StudentId);
        submittedHomeworksCount.Should().Be(0);
    }

    [Fact]
    public async Task V1CreateSubmittedHomework_SubmittedHomeworkAlreadyExists_ShouldBeOtherErrorConflict()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);
        await AddGroupStudentInDbAsync(groupId, studentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published);
        await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);
        var request = _fixture.Build<V1CreateSubmittedHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await CreateSubmittedHomeworkClient.V1CreateSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.Conflict);
        response.OtherError.Message.Should().Be("Домашнее задание уже было отправлено");

        var submittedHomeworksCount = await GetSubmittedHomeworksCountAsync(request.HomeworkId, request.StudentId);
        submittedHomeworksCount.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1CreateSubmittedHomework_NotPositiveHomeworkId_ShouldReturnInvalidArgument(long homeworkId)
    {
        // Arrange
        var request = _fixture.Build<V1CreateSubmittedHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .Create();

        // Act
        var act = async () => await CreateSubmittedHomeworkClient.V1CreateSubmittedHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.HomeworkId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1CreateSubmittedHomework_NotPositiveStudentId_ShouldReturnInvalidArgument(long studentId)
    {
        // Arrange
        var request = _fixture.Build<V1CreateSubmittedHomeworkRequest>()
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var act = async () => await CreateSubmittedHomeworkClient.V1CreateSubmittedHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StudentId));
    }

    private async Task<(long HomeworkId, long StudentId, string? Comment)> GetSubmittedHomeworkAsync(long submittedHomeworkId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        var row = await connection.QuerySingleAsync(
            "select homework_id, student_id, comment from submitted_homeworks where id = @submittedHomeworkId",
            new { submittedHomeworkId });
        return (row.homework_id, row.student_id, row.comment);
    }

    private async Task<int> GetSubmittedHomeworksCountAsync(long homeworkId, long studentId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.QuerySingleAsync<int>(
            "select count(*) from submitted_homeworks where homework_id = @homeworkId and student_id = @studentId",
            new { homeworkId, studentId });
    }
}
