using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using HomeworkStatusModel = Peerly.Core.Models.Homeworks.HomeworkStatus;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.DeleteSubmittedHomework;

public sealed class DeleteSubmittedHomeworkIntegrationTests : DeleteSubmittedHomeworkIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public DeleteSubmittedHomeworkIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1DeleteSubmittedHomework_PublishedHomeworkAndOwnerStudent_ShouldDeleteSubmittedHomeworkAndFiles()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);
        await AddSubmittedHomeworkFileInDbAsync(submittedHomeworkId, _fixture.Create<long>());
        var request = _fixture.Build<V1DeleteSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await DeleteSubmittedHomeworkClient.V1DeleteSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteSubmittedHomeworkResponse.ResponseOneofCase.SuccessResponse);

        var submittedHomeworksCount = await GetSubmittedHomeworksCountAsync(submittedHomeworkId);
        submittedHomeworksCount.Should().Be(0);
        var submittedHomeworkFilesCount = await GetSubmittedHomeworkFilesCountAsync(submittedHomeworkId);
        submittedHomeworkFilesCount.Should().Be(0);
    }

    [Fact]
    public async Task V1DeleteSubmittedHomework_SubmittedHomeworkNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var request = _fixture.Build<V1DeleteSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, _fixture.Create<long>())
            .With(result => result.StudentId, _fixture.Create<long>())
            .Create();

        // Act
        var response = await DeleteSubmittedHomeworkClient.V1DeleteSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteSubmittedHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Отправленный ответ к домашнему заданию не найден");
    }

    [Fact]
    public async Task V1DeleteSubmittedHomework_SubmittedHomeworkBelongsToAnotherStudent_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var ownerStudentId = _fixture.Create<long>();
        var requestingStudentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(requestingStudentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId);
        await AddSubmittedHomeworkFileInDbAsync(submittedHomeworkId, _fixture.Create<long>());
        var request = _fixture.Build<V1DeleteSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, requestingStudentId)
            .Create();

        // Act
        var response = await DeleteSubmittedHomeworkClient.V1DeleteSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteSubmittedHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Отправленный ответ к домашнему заданию не найден");

        var submittedHomeworksCount = await GetSubmittedHomeworksCountAsync(submittedHomeworkId);
        submittedHomeworksCount.Should().Be(1);
        var submittedHomeworkFilesCount = await GetSubmittedHomeworkFilesCountAsync(submittedHomeworkId);
        submittedHomeworkFilesCount.Should().Be(1);
    }

    [Fact]
    public async Task V1DeleteSubmittedHomework_LinkedHomeworkMissing_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var studentId = _fixture.Create<long>();
        var missingHomeworkId = _fixture.Create<long>();

        await AddStudentInDbAsync(studentId);

        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(missingHomeworkId, studentId);
        await AddSubmittedHomeworkFileInDbAsync(submittedHomeworkId, _fixture.Create<long>());
        var request = _fixture.Build<V1DeleteSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await DeleteSubmittedHomeworkClient.V1DeleteSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteSubmittedHomeworkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Домашнее задание не найдено");

        var submittedHomeworksCount = await GetSubmittedHomeworksCountAsync(submittedHomeworkId);
        submittedHomeworksCount.Should().Be(1);
        var submittedHomeworkFilesCount = await GetSubmittedHomeworkFilesCountAsync(submittedHomeworkId);
        submittedHomeworkFilesCount.Should().Be(1);
    }

    [Theory]
    [InlineData(HomeworkStatusModel.Draft)]
    [InlineData(HomeworkStatusModel.Reviewing)]
    [InlineData(HomeworkStatusModel.Confirmation)]
    [InlineData(HomeworkStatusModel.Finished)]
    public async Task V1DeleteSubmittedHomework_HomeworkNotInPublishedStatus_ShouldBeOtherErrorConflict(HomeworkStatusModel status)
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, status);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);
        await AddSubmittedHomeworkFileInDbAsync(submittedHomeworkId, _fixture.Create<long>());
        var request = _fixture.Build<V1DeleteSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await DeleteSubmittedHomeworkClient.V1DeleteSubmittedHomeworkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteSubmittedHomeworkResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainSingle("Отправка ответов для домашнего задания закрыта");

        var submittedHomeworksCount = await GetSubmittedHomeworksCountAsync(submittedHomeworkId);
        submittedHomeworksCount.Should().Be(1);
        var submittedHomeworkFilesCount = await GetSubmittedHomeworkFilesCountAsync(submittedHomeworkId);
        submittedHomeworkFilesCount.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1DeleteSubmittedHomework_NotPositiveSubmittedHomeworkId_ShouldReturnInvalidArgument(long submittedHomeworkId)
    {
        // Arrange
        var request = _fixture.Build<V1DeleteSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .Create();

        // Act
        var act = async () => await DeleteSubmittedHomeworkClient.V1DeleteSubmittedHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.SubmittedHomeworkId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1DeleteSubmittedHomework_NotPositiveStudentId_ShouldReturnInvalidArgument(long studentId)
    {
        // Arrange
        var request = _fixture.Build<V1DeleteSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, _fixture.Create<long>())
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var act = async () => await DeleteSubmittedHomeworkClient.V1DeleteSubmittedHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StudentId));
    }

    private async Task<int> GetSubmittedHomeworksCountAsync(long submittedHomeworkId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.QuerySingleAsync<int>(
            "select count(*) from submitted_homeworks where id = @submittedHomeworkId",
            new { submittedHomeworkId });
    }

    private async Task<int> GetSubmittedHomeworkFilesCountAsync(long submittedHomeworkId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.QuerySingleAsync<int>(
            "select count(*) from submitted_homework_files where submitted_homework_id = @submittedHomeworkId",
            new { submittedHomeworkId });
    }
}
