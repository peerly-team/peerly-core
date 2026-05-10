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

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.CreateSubmittedHomeworkFile;

public sealed class CreateSubmittedHomeworkFileIntegrationTests : CreateSubmittedHomeworkFileIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public CreateSubmittedHomeworkFileIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1CreateSubmittedHomeworkFile_CourseHomeworkUnsupportedExtension_ShouldCreateFile()
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
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);

        var storageId = Guid.NewGuid();
        var request = new V1CreateSubmittedHomeworkFileRequest
        {
            SubmittedHomeworkId = submittedHomeworkId,
            StorageId = storageId.ToString(),
            FileName = "report.pdf",
            FileSize = 1024,
            StudentId = studentId
        };

        // Act
        var response = await CreateSubmittedHomeworkFileClient.V1CreateSubmittedHomeworkFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedHomeworkFileResponse.ResponseOneofCase.SuccessResponse);
        response.SuccessResponse.FileId.Should().BeGreaterThan(0);

        var file = await GetFileAsync(response.SuccessResponse.FileId);
        file.StorageId.Should().Be(storageId);
        file.Name.Should().Be("report.pdf");
        file.Size.Should().Be(1024);

        var submittedHomeworkFileExists = await GetSubmittedHomeworkFileAsync(submittedHomeworkId, response.SuccessResponse.FileId);
        submittedHomeworkFileExists.Should().BeTrue();
    }

    [Fact]
    public async Task V1CreateSubmittedHomeworkFile_GroupHomeworkUnsupportedExtension_ShouldCreateFile()
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
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);

        var request = new V1CreateSubmittedHomeworkFileRequest
        {
            SubmittedHomeworkId = submittedHomeworkId,
            StorageId = Guid.NewGuid().ToString(),
            FileName = "homework.docx",
            FileSize = 2048,
            StudentId = studentId
        };

        // Act
        var response = await CreateSubmittedHomeworkFileClient.V1CreateSubmittedHomeworkFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedHomeworkFileResponse.ResponseOneofCase.SuccessResponse);
        response.SuccessResponse.FileId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task V1CreateSubmittedHomeworkFile_SubmittedHomeworkNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var request = new V1CreateSubmittedHomeworkFileRequest
        {
            SubmittedHomeworkId = _fixture.Create<long>(),
            StorageId = Guid.NewGuid().ToString(),
            FileName = "file.pdf",
            FileSize = 100,
            StudentId = _fixture.Create<long>()
        };

        // Act
        var response = await CreateSubmittedHomeworkFileClient.V1CreateSubmittedHomeworkFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedHomeworkFileResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Отправленный ответ к домашнему заданию не найден");
    }

    [Fact]
    public async Task V1CreateSubmittedHomeworkFile_WrongStudent_ShouldBeOtherErrorNotFound()
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
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);

        var request = new V1CreateSubmittedHomeworkFileRequest
        {
            SubmittedHomeworkId = submittedHomeworkId,
            StorageId = Guid.NewGuid().ToString(),
            FileName = "file.pdf",
            FileSize = 100,
            StudentId = otherStudentId
        };

        // Act
        var response = await CreateSubmittedHomeworkFileClient.V1CreateSubmittedHomeworkFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedHomeworkFileResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Отправленный ответ к домашнему заданию не найден");
    }

    [Fact]
    public async Task V1CreateSubmittedHomeworkFile_HomeworkDraft_ShouldBeOtherErrorNotFound()
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
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);

        var request = new V1CreateSubmittedHomeworkFileRequest
        {
            SubmittedHomeworkId = submittedHomeworkId,
            StorageId = Guid.NewGuid().ToString(),
            FileName = "file.pdf",
            FileSize = 100,
            StudentId = studentId
        };

        // Act
        var response = await CreateSubmittedHomeworkFileClient.V1CreateSubmittedHomeworkFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedHomeworkFileResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Домашнее задание не найдено");
    }

    [Theory]
    [InlineData(HomeworkStatusModel.Reviewing)]
    [InlineData(HomeworkStatusModel.Confirmation)]
    [InlineData(HomeworkStatusModel.Finished)]
    public async Task V1CreateSubmittedHomeworkFile_HomeworkNotPublished_ShouldBeOtherErrorConflict(HomeworkStatusModel status)
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
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);

        var request = new V1CreateSubmittedHomeworkFileRequest
        {
            SubmittedHomeworkId = submittedHomeworkId,
            StorageId = Guid.NewGuid().ToString(),
            FileName = "file.pdf",
            FileSize = 100,
            StudentId = studentId
        };

        // Act
        var response = await CreateSubmittedHomeworkFileClient.V1CreateSubmittedHomeworkFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedHomeworkFileResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.Conflict);
        response.OtherError.Message.Should().Be("Отправка ответов для домашнего задания закрыта");
    }

    [Fact]
    public async Task V1CreateSubmittedHomeworkFile_DeadlinePassed_ShouldBeOtherErrorConflict()
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
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);

        var request = new V1CreateSubmittedHomeworkFileRequest
        {
            SubmittedHomeworkId = submittedHomeworkId,
            StorageId = Guid.NewGuid().ToString(),
            FileName = "file.pdf",
            FileSize = 100,
            StudentId = studentId
        };

        // Act
        var response = await CreateSubmittedHomeworkFileClient.V1CreateSubmittedHomeworkFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedHomeworkFileResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.Conflict);
        response.OtherError.Message.Should().Be("Отправка ответов для домашнего задания закрыта");
    }

    [Fact]
    public async Task V1CreateSubmittedHomeworkFile_CourseStudentNotInCourse_ShouldBeOtherErrorNotFound()
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
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, otherStudentId);

        var request = new V1CreateSubmittedHomeworkFileRequest
        {
            SubmittedHomeworkId = submittedHomeworkId,
            StorageId = Guid.NewGuid().ToString(),
            FileName = "file.pdf",
            FileSize = 100,
            StudentId = otherStudentId
        };

        // Act
        var response = await CreateSubmittedHomeworkFileClient.V1CreateSubmittedHomeworkFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedHomeworkFileResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Домашнее задание не найдено");
    }

    [Fact]
    public async Task V1CreateSubmittedHomeworkFile_GroupStudentNotInGroup_ShouldBeOtherErrorNotFound()
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
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);

        var request = new V1CreateSubmittedHomeworkFileRequest
        {
            SubmittedHomeworkId = submittedHomeworkId,
            StorageId = Guid.NewGuid().ToString(),
            FileName = "file.pdf",
            FileSize = 100,
            StudentId = studentId
        };

        // Act
        var response = await CreateSubmittedHomeworkFileClient.V1CreateSubmittedHomeworkFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateSubmittedHomeworkFileResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Домашнее задание не найдено");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1CreateSubmittedHomeworkFile_NotPositiveSubmittedHomeworkId_ShouldReturnInvalidArgument(long submittedHomeworkId)
    {
        // Arrange
        var request = CreateValidRequest();
        request.SubmittedHomeworkId = submittedHomeworkId;

        // Act
        var act = async () => await CreateSubmittedHomeworkFileClient.V1CreateSubmittedHomeworkFileAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.SubmittedHomeworkId));
    }

    [Fact]
    public async Task V1CreateSubmittedHomeworkFile_InvalidStorageId_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = CreateValidRequest();
        request.StorageId = "not-guid";

        // Act
        var act = async () => await CreateSubmittedHomeworkFileClient.V1CreateSubmittedHomeworkFileAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StorageId));
    }

    [Fact]
    public async Task V1CreateSubmittedHomeworkFile_EmptyFileName_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = CreateValidRequest();
        request.FileName = string.Empty;

        // Act
        var act = async () => await CreateSubmittedHomeworkFileClient.V1CreateSubmittedHomeworkFileAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.FileName));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1CreateSubmittedHomeworkFile_NotPositiveFileSize_ShouldReturnInvalidArgument(int fileSize)
    {
        // Arrange
        var request = CreateValidRequest();
        request.FileSize = fileSize;

        // Act
        var act = async () => await CreateSubmittedHomeworkFileClient.V1CreateSubmittedHomeworkFileAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.FileSize));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1CreateSubmittedHomeworkFile_NotPositiveStudentId_ShouldReturnInvalidArgument(long studentId)
    {
        // Arrange
        var request = CreateValidRequest();
        request.StudentId = studentId;

        // Act
        var act = async () => await CreateSubmittedHomeworkFileClient.V1CreateSubmittedHomeworkFileAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StudentId));
    }

    private V1CreateSubmittedHomeworkFileRequest CreateValidRequest()
    {
        return new V1CreateSubmittedHomeworkFileRequest
        {
            SubmittedHomeworkId = _fixture.Create<long>(),
            StorageId = Guid.NewGuid().ToString(),
            FileName = "file.pdf",
            FileSize = 1024,
            StudentId = _fixture.Create<long>()
        };
    }

    private async Task<FileRecord> GetFileAsync(long fileId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.QuerySingleAsync<FileRecord>(
            "select storage_id, name, size from files where id = @fileId",
            new { fileId });
    }

    private async Task<bool> GetSubmittedHomeworkFileAsync(long submittedHomeworkId, long fileId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(
            """
            select exists(
                select from submitted_homework_files
                where submitted_homework_id = @submittedHomeworkId
                  and file_id = @fileId)
            """,
            new { submittedHomeworkId, fileId });
    }

    private sealed record FileRecord
    {
        public Guid Storage_id { get; init; }
        public string Name { get; init; } = null!;
        public int Size { get; init; }

        public Guid StorageId => Storage_id;
    }
}
