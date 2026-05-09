using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using HomeworkStatusModel = Peerly.Core.Models.Homeworks.HomeworkStatus;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.DeleteHomework;

public sealed class DeleteHomeworkIntegrationTests : DeleteHomeworkIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public DeleteHomeworkIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1DeleteHomework_HomeworkTeacherMatchesAndHomeworkInDraftStatus_ShouldDeleteHomeworkAndFiles()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Draft);
        var request = _fixture.Build<V1DeleteHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);
        await AddHomeworkFileInDbAsync(homeworkId, _fixture.Create<long>(), teacherId);

        // Act
        var response = await DeleteHomeworkClient.V1DeleteHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteHomeworkResponse.ResponseOneofCase.SuccessResponse);

        var homeworksCount = await GetHomeworksCountAsync(homeworkId);
        homeworksCount.Should().Be(0);
        var homeworkFilesCount = await GetHomeworkFilesCountAsync(homeworkId);
        homeworkFilesCount.Should().Be(0);
    }

    [Fact]
    public async Task V1DeleteHomework_HomeworkNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var request = _fixture.Build<V1DeleteHomeworkRequest>()
            .With(result => result.HomeworkId, _fixture.Create<long>())
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);

        // Act
        var response = await DeleteHomeworkClient.V1DeleteHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Домашнее задание не найдено");
    }

    [Fact]
    public async Task V1DeleteHomework_HomeworkTeacherDoesNotMatch_ShouldBeOtherErrorPermissionDenied()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var otherTeacherId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var homeworkId = await AddHomeworkInDbAsync(courseId, otherTeacherId, HomeworkStatusModel.Draft);
        var request = _fixture.Build<V1DeleteHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);
        await AddTeacherInDbAsync(otherTeacherId);
        await AddHomeworkFileInDbAsync(homeworkId, _fixture.Create<long>(), otherTeacherId);

        // Act
        var response = await DeleteHomeworkClient.V1DeleteHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.PermissionDenied);
        response.OtherError.Message.Should().BeNull();

        var homeworksCount = await GetHomeworksCountAsync(homeworkId);
        homeworksCount.Should().Be(1);
        var homeworkFilesCount = await GetHomeworkFilesCountAsync(homeworkId);
        homeworkFilesCount.Should().Be(1);
    }

    [Theory]
    [InlineData(HomeworkStatusModel.Published)]
    [InlineData(HomeworkStatusModel.Reviewing)]
    [InlineData(HomeworkStatusModel.Confirmation)]
    [InlineData(HomeworkStatusModel.Finished)]
    public async Task V1DeleteHomework_HomeworkNotInDraftStatus_ShouldBeValidationError(HomeworkStatusModel status)
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, status);
        var request = _fixture.Build<V1DeleteHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);
        await AddHomeworkFileInDbAsync(homeworkId, _fixture.Create<long>(), teacherId);

        // Act
        var response = await DeleteHomeworkClient.V1DeleteHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteHomeworkResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainSingle("Удалить домашнее задание можно только в статусе \"Черновик\"");

        var homeworksCount = await GetHomeworksCountAsync(homeworkId);
        homeworksCount.Should().Be(1);
        var homeworkFilesCount = await GetHomeworkFilesCountAsync(homeworkId);
        homeworkFilesCount.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1DeleteHomework_NotPositiveHomeworkId_ShouldReturnInvalidArgument(long homeworkId)
    {
        // Arrange
        var request = _fixture.Build<V1DeleteHomeworkRequest>()
            .With(result => result.HomeworkId, homeworkId)
            .Create();

        // Act
        var act = async () => await DeleteHomeworkClient.V1DeleteHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.HomeworkId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1DeleteHomework_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = _fixture.Build<V1DeleteHomeworkRequest>()
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var act = async () => await DeleteHomeworkClient.V1DeleteHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }

    private async Task<int> GetHomeworksCountAsync(long homeworkId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.QuerySingleAsync<int>(
            "select count(*) from homeworks where id = @homeworkId",
            new { homeworkId });
    }

    private async Task<int> GetHomeworkFilesCountAsync(long homeworkId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.QuerySingleAsync<int>(
            "select count(*) from homework_files where homework_id = @homeworkId",
            new { homeworkId });
    }
}
