using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using HomeworkStatusModel = Peerly.Core.Models.Homeworks.HomeworkStatus;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.DeleteSubmittedHomeworkFile;

public sealed class DeleteSubmittedHomeworkFileIntegrationTests : DeleteSubmittedHomeworkFileIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public DeleteSubmittedHomeworkFileIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1DeleteSubmittedHomeworkFile_PublishedHomeworkAndOwnerStudent_ShouldDeleteOnlyTargetFile()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var fileId = _fixture.Create<long>();
        var otherFileId = fileId + 1;
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);
        await AddSubmittedHomeworkFileInDbAsync(submittedHomeworkId, fileId);
        await AddSubmittedHomeworkFileInDbAsync(submittedHomeworkId, otherFileId);
        var request = _fixture.Build<V1DeleteSubmittedHomeworkFileRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.FileId, fileId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await DeleteSubmittedHomeworkFileClient.V1DeleteSubmittedHomeworkFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteSubmittedHomeworkFileResponse.ResponseOneofCase.SuccessResponse);

        var targetSubmittedHomeworkFilesCount = await GetSubmittedHomeworkFilesCountAsync(submittedHomeworkId, fileId);
        targetSubmittedHomeworkFilesCount.Should().Be(0);
        var otherSubmittedHomeworkFilesCount = await GetSubmittedHomeworkFilesCountAsync(submittedHomeworkId, otherFileId);
        otherSubmittedHomeworkFilesCount.Should().Be(1);
    }

    [Fact]
    public async Task V1DeleteSubmittedHomeworkFile_SubmittedHomeworkBelongsToAnotherStudent_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var ownerStudentId = _fixture.Create<long>();
        var requestingStudentId = _fixture.Create<long>();
        var fileId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(requestingStudentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId);
        await AddSubmittedHomeworkFileInDbAsync(submittedHomeworkId, fileId);
        var request = _fixture.Build<V1DeleteSubmittedHomeworkFileRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.FileId, fileId)
            .With(result => result.StudentId, requestingStudentId)
            .Create();

        // Act
        var response = await DeleteSubmittedHomeworkFileClient.V1DeleteSubmittedHomeworkFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteSubmittedHomeworkFileResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Отправленный ответ к домашнему заданию не найден");

        var submittedHomeworkFilesCount = await GetSubmittedHomeworkFilesCountAsync(submittedHomeworkId, fileId);
        submittedHomeworkFilesCount.Should().Be(1);
    }

    [Fact]
    public async Task V1DeleteSubmittedHomeworkFile_FileNotAttachedToSubmittedHomework_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var fileId = _fixture.Create<long>();
        var otherFileId = fileId + 1;
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);
        await AddSubmittedHomeworkFileInDbAsync(submittedHomeworkId, otherFileId);
        var request = _fixture.Build<V1DeleteSubmittedHomeworkFileRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.FileId, fileId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await DeleteSubmittedHomeworkFileClient.V1DeleteSubmittedHomeworkFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteSubmittedHomeworkFileResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Файл отправленного ответа к домашнему заданию не найден");

        var otherSubmittedHomeworkFilesCount = await GetSubmittedHomeworkFilesCountAsync(submittedHomeworkId, otherFileId);
        otherSubmittedHomeworkFilesCount.Should().Be(1);
    }

    [Fact]
    public async Task V1DeleteSubmittedHomeworkFile_LinkedHomeworkMissing_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var studentId = _fixture.Create<long>();
        var fileId = _fixture.Create<long>();
        var missingHomeworkId = _fixture.Create<long>();

        await AddStudentInDbAsync(studentId);

        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(missingHomeworkId, studentId);
        await AddSubmittedHomeworkFileInDbAsync(submittedHomeworkId, fileId);
        var request = _fixture.Build<V1DeleteSubmittedHomeworkFileRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.FileId, fileId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await DeleteSubmittedHomeworkFileClient.V1DeleteSubmittedHomeworkFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteSubmittedHomeworkFileResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Домашнее задание не найдено");

        var submittedHomeworkFilesCount = await GetSubmittedHomeworkFilesCountAsync(submittedHomeworkId, fileId);
        submittedHomeworkFilesCount.Should().Be(1);
    }

    [Theory]
    [InlineData(HomeworkStatusModel.Draft)]
    [InlineData(HomeworkStatusModel.Reviewing)]
    [InlineData(HomeworkStatusModel.Confirmation)]
    [InlineData(HomeworkStatusModel.Finished)]
    public async Task V1DeleteSubmittedHomeworkFile_HomeworkNotInPublishedStatus_ShouldBeValidationError(HomeworkStatusModel status)
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var fileId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, status);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);
        await AddSubmittedHomeworkFileInDbAsync(submittedHomeworkId, fileId);
        var request = _fixture.Build<V1DeleteSubmittedHomeworkFileRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.FileId, fileId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await DeleteSubmittedHomeworkFileClient.V1DeleteSubmittedHomeworkFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteSubmittedHomeworkFileResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainSingle("Отправка ответов для домашнего задания закрыта");

        var submittedHomeworkFilesCount = await GetSubmittedHomeworkFilesCountAsync(submittedHomeworkId, fileId);
        submittedHomeworkFilesCount.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1DeleteSubmittedHomeworkFile_NotPositiveSubmittedHomeworkId_ShouldReturnInvalidArgument(long submittedHomeworkId)
    {
        // Arrange
        var request = _fixture.Build<V1DeleteSubmittedHomeworkFileRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.FileId, 1)
            .With(result => result.StudentId, 1)
            .Create();

        // Act
        var act = async () => await DeleteSubmittedHomeworkFileClient.V1DeleteSubmittedHomeworkFileAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.SubmittedHomeworkId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1DeleteSubmittedHomeworkFile_NotPositiveFileId_ShouldReturnInvalidArgument(long fileId)
    {
        // Arrange
        var request = _fixture.Build<V1DeleteSubmittedHomeworkFileRequest>()
            .With(result => result.SubmittedHomeworkId, 1)
            .With(result => result.FileId, fileId)
            .With(result => result.StudentId, 1)
            .Create();

        // Act
        var act = async () => await DeleteSubmittedHomeworkFileClient.V1DeleteSubmittedHomeworkFileAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.FileId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1DeleteSubmittedHomeworkFile_NotPositiveStudentId_ShouldReturnInvalidArgument(long studentId)
    {
        // Arrange
        var request = _fixture.Build<V1DeleteSubmittedHomeworkFileRequest>()
            .With(result => result.SubmittedHomeworkId, 1)
            .With(result => result.FileId, 1)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var act = async () => await DeleteSubmittedHomeworkFileClient.V1DeleteSubmittedHomeworkFileAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StudentId));
    }

    private async Task<int> GetSubmittedHomeworkFilesCountAsync(long submittedHomeworkId, long fileId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.QuerySingleAsync<int>(
            "select count(*) from submitted_homework_files where submitted_homework_id = @submittedHomeworkId and file_id = @fileId",
            new { submittedHomeworkId, fileId });
    }
}
