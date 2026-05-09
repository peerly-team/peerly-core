using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using HomeworkStatusModel = Peerly.Core.Models.Homeworks.HomeworkStatus;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.DeleteHomeworkFile;

public sealed class DeleteHomeworkFileIntegrationTests : DeleteHomeworkFileIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public DeleteHomeworkFileIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1DeleteHomeworkFile_HomeworkTeacherMatchesAndHomeworkInDraftStatus_ShouldDeleteOnlyTargetHomeworkFile()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var fileId = _fixture.Create<long>();
        var otherFileId = fileId + 1;
        var courseId = await AddCourseInDbAsync();
        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Draft);
        var request = _fixture.Build<V1DeleteHomeworkFileRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.FileId, fileId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);
        await AddHomeworkFileInDbAsync(homeworkId, fileId, teacherId);
        await AddHomeworkFileInDbAsync(homeworkId, otherFileId, teacherId);

        // Act
        var response = await DeleteHomeworkFileClient.V1DeleteHomeworkFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteHomeworkFileResponse.ResponseOneofCase.SuccessResponse);

        var targetHomeworkFilesCount = await GetHomeworkFilesCountAsync(homeworkId, fileId);
        targetHomeworkFilesCount.Should().Be(0);
        var otherHomeworkFilesCount = await GetHomeworkFilesCountAsync(homeworkId, otherFileId);
        otherHomeworkFilesCount.Should().Be(1);
    }

    [Fact]
    public async Task V1DeleteHomeworkFile_HomeworkNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var request = _fixture.Build<V1DeleteHomeworkFileRequest>()
            .With(result => result.HomeworkId, _fixture.Create<long>())
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);

        // Act
        var response = await DeleteHomeworkFileClient.V1DeleteHomeworkFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteHomeworkFileResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Домашнее задание не найдено");
    }

    [Fact]
    public async Task V1DeleteHomeworkFile_HomeworkTeacherDoesNotMatch_ShouldBeOtherErrorPermissionDenied()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var otherTeacherId = _fixture.Create<long>();
        var fileId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var homeworkId = await AddHomeworkInDbAsync(courseId, otherTeacherId, HomeworkStatusModel.Draft);
        var request = _fixture.Build<V1DeleteHomeworkFileRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.FileId, fileId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);
        await AddTeacherInDbAsync(otherTeacherId);
        await AddHomeworkFileInDbAsync(homeworkId, fileId, otherTeacherId);

        // Act
        var response = await DeleteHomeworkFileClient.V1DeleteHomeworkFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteHomeworkFileResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.PermissionDenied);
        response.OtherError.Message.Should().BeNull();

        var homeworkFilesCount = await GetHomeworkFilesCountAsync(homeworkId, fileId);
        homeworkFilesCount.Should().Be(1);
    }

    [Theory]
    [InlineData(HomeworkStatusModel.Published)]
    [InlineData(HomeworkStatusModel.Reviewing)]
    [InlineData(HomeworkStatusModel.Confirmation)]
    [InlineData(HomeworkStatusModel.Finished)]
    public async Task V1DeleteHomeworkFile_HomeworkNotInDraftStatus_ShouldBeValidationError(HomeworkStatusModel status)
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var fileId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, status);
        var request = _fixture.Build<V1DeleteHomeworkFileRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.FileId, fileId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);
        await AddHomeworkFileInDbAsync(homeworkId, fileId, teacherId);

        // Act
        var response = await DeleteHomeworkFileClient.V1DeleteHomeworkFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteHomeworkFileResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainSingle("Открепить файл можно когда домашнее задание в статусе \"Черновик\"");

        var homeworkFilesCount = await GetHomeworkFilesCountAsync(homeworkId, fileId);
        homeworkFilesCount.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1DeleteHomeworkFile_NotPositiveHomeworkId_ShouldReturnInvalidArgument(long homeworkId)
    {
        // Arrange
        var request = _fixture.Build<V1DeleteHomeworkFileRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.FileId, 1)
            .With(result => result.TeacherId, 1)
            .Create();

        // Act
        var act = async () => await DeleteHomeworkFileClient.V1DeleteHomeworkFileAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.HomeworkId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1DeleteHomeworkFile_NotPositiveFileId_ShouldReturnInvalidArgument(long fileId)
    {
        // Arrange
        var request = _fixture.Build<V1DeleteHomeworkFileRequest>()
            .With(result => result.HomeworkId, 1)
            .With(result => result.FileId, fileId)
            .With(result => result.TeacherId, 1)
            .Create();

        // Act
        var act = async () => await DeleteHomeworkFileClient.V1DeleteHomeworkFileAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.FileId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1DeleteHomeworkFile_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = _fixture.Build<V1DeleteHomeworkFileRequest>()
            .With(result => result.HomeworkId, 1)
            .With(result => result.FileId, 1)
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var act = async () => await DeleteHomeworkFileClient.V1DeleteHomeworkFileAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }

    private async Task<int> GetHomeworkFilesCountAsync(long homeworkId, long fileId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.QuerySingleAsync<int>(
            "select count(*) from homework_files where homework_id = @homeworkId and file_id = @fileId",
            new { homeworkId, fileId });
    }
}
