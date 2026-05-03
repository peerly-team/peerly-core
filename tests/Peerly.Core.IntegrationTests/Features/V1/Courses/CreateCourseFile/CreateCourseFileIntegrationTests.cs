using System;
using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.CreateCourseFile;

public sealed class CreateCourseFileIntegrationTests : CreateCourseFileIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public CreateCourseFileIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1CreateCourseFile_CourseTeacherExistsAndCourseExists_ShouldCreateCourseFile()
    {
        // Arrange
        var courseId = _fixture.Create<long>();
        var teacherId = _fixture.Create<long>();
        var storageId = _fixture.Create<Guid>();
        var request = CreateValidRequest(courseId, teacherId, storageId);

        await AddCourseInDbAsync(courseId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        // Act
        var response = await CreateCourseFileClient.V1CreateCourseFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateCourseFileResponse.ResponseOneofCase.SuccessResponse);
        response.SuccessResponse.FileId.Should().BePositive();

        var file = await GetCreatedFileAsync(response.SuccessResponse.FileId);
        file.Should().BeEquivalentTo((storageId, request.FileName, request.FileSize));

        var isCourseFileExists = await IsCourseFileExistsAsync(courseId, response.SuccessResponse.FileId, teacherId);
        isCourseFileExists.Should().BeTrue();
    }

    [Fact]
    public async Task V1CreateCourseFile_CourseTeacherNotFound_ShouldBeOtherErrorPermissionDenied()
    {
        // Arrange
        var courseId = _fixture.Create<long>();
        var teacherId = _fixture.Create<long>();
        var storageId = _fixture.Create<Guid>();
        var request = CreateValidRequest(courseId, teacherId, storageId);

        await AddCourseInDbAsync(courseId);

        // Act
        var response = await CreateCourseFileClient.V1CreateCourseFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateCourseFileResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.PermissionDenied);
        response.OtherError.Message.Should().BeNull();

        var isFileExists = await IsFileExistsAsync(storageId);
        isFileExists.Should().BeFalse();
    }

    [Fact]
    public async Task V1CreateCourseFile_CourseNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var courseId = _fixture.Create<long>();
        var teacherId = _fixture.Create<long>();
        var storageId = _fixture.Create<Guid>();
        var request = CreateValidRequest(courseId, teacherId, storageId);

        await AddCourseTeacherInDbAsync(courseId, teacherId);

        // Act
        var response = await CreateCourseFileClient.V1CreateCourseFileAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateCourseFileResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Курс не найден");

        var isFileExists = await IsFileExistsAsync(storageId);
        isFileExists.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1CreateCourseFile_NotPositiveCourseId_ShouldReturnInvalidArgument(long courseId)
    {
        // Arrange
        var request = CreateValidRequest(courseId, _fixture.Create<long>(), _fixture.Create<Guid>());

        // Act
        var act = async () => await CreateCourseFileClient.V1CreateCourseFileAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.CourseId));
    }

    [Fact]
    public async Task V1CreateCourseFile_InvalidStorageId_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = CreateValidRequest(_fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<Guid>());
        request.StorageId = "not-guid";

        // Act
        var act = async () => await CreateCourseFileClient.V1CreateCourseFileAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StorageId));
    }

    [Fact]
    public async Task V1CreateCourseFile_EmptyFileName_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = CreateValidRequest(_fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<Guid>());
        request.FileName = string.Empty;

        // Act
        var act = async () => await CreateCourseFileClient.V1CreateCourseFileAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.FileName));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1CreateCourseFile_NotPositiveFileSize_ShouldReturnInvalidArgument(int fileSize)
    {
        // Arrange
        var request = CreateValidRequest(_fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<Guid>());
        request.FileSize = fileSize;

        // Act
        var act = async () => await CreateCourseFileClient.V1CreateCourseFileAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.FileSize));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1CreateCourseFile_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = CreateValidRequest(_fixture.Create<long>(), teacherId, _fixture.Create<Guid>());

        // Act
        var act = async () => await CreateCourseFileClient.V1CreateCourseFileAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }

    private V1CreateCourseFileRequest CreateValidRequest(long courseId, long teacherId, Guid storageId)
    {
        return _fixture.Build<V1CreateCourseFileRequest>()
            .With(result => result.CourseId, courseId)
            .With(result => result.TeacherId, teacherId)
            .With(result => result.StorageId, storageId.ToString())
            .With(result => result.FileSize, 1024)
            .Create();
    }

    private async Task<(Guid StorageId, string Name, int Size)> GetCreatedFileAsync(long fileId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        var row = await connection.QuerySingleAsync<FileDb>(
            """
            select storage_id as StorageId,
                   name as Name,
                   size as Size
              from files
             where id = @fileId
            """,
            new { fileId });
        return (row.StorageId, row.Name, row.Size);
    }

    private async Task<bool> IsCourseFileExistsAsync(long courseId, long fileId, long teacherId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(
            """
            select exists(select
                            from course_files
                           where course_id = @courseId
                             and file_id = @fileId
                             and teacher_id = @teacherId)
            """,
            new { courseId, fileId, teacherId });
    }

    private async Task<bool> IsFileExistsAsync(Guid storageId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(
            "select exists(select from files where storage_id = @storageId)",
            new { storageId });
    }

    private sealed record FileDb
    {
        public required Guid StorageId { get; init; }
        public required string Name { get; init; }
        public required int Size { get; init; }
    }
}
