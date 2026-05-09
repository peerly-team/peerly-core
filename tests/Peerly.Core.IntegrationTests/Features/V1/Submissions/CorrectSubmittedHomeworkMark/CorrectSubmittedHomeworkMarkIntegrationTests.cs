using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using HomeworkStatusModel = Peerly.Core.Models.Homeworks.HomeworkStatus;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.CorrectSubmittedHomeworkMark;

public sealed class CorrectSubmittedHomeworkMarkIntegrationTests : CorrectSubmittedHomeworkMarkIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public CorrectSubmittedHomeworkMarkIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1CorrectSubmittedHomeworkMark_CourseTeacherExistsAndHomeworkConfirmation_ShouldSucceed()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Confirmation);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);
        await AddSubmittedHomeworkMarkInDbAsync(submittedHomeworkId);

        var expectedMark = _fixture.Create<int>() % 100;
        var request = new V1CorrectSubmittedHomeworkMarkRequest
        {
            SubmittedHomeworkId = submittedHomeworkId,
            TeacherId = teacherId,
            TeacherMark = expectedMark
        };

        // Act
        var response = await CorrectSubmittedHomeworkMarkClient.V1CorrectSubmittedHomeworkMarkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CorrectSubmittedHomeworkMarkResponse.ResponseOneofCase.SuccessResponse);

        var (teacherMark, dbTeacherId) = await GetMarkAsync(submittedHomeworkId);
        teacherMark.Should().Be(expectedMark);
        dbTeacherId.Should().Be(teacherId);
    }

    [Fact]
    public async Task V1CorrectSubmittedHomeworkMark_GroupTeacherExistsAndHomeworkConfirmation_ShouldSucceed()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);
        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Confirmation, groupId);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);
        await AddGroupTeacherInDbAsync(groupId, teacherId);
        await AddSubmittedHomeworkMarkInDbAsync(submittedHomeworkId);

        var expectedMark = _fixture.Create<int>() % 100;
        var request = new V1CorrectSubmittedHomeworkMarkRequest
        {
            SubmittedHomeworkId = submittedHomeworkId,
            TeacherId = teacherId,
            TeacherMark = expectedMark
        };

        // Act
        var response = await CorrectSubmittedHomeworkMarkClient.V1CorrectSubmittedHomeworkMarkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CorrectSubmittedHomeworkMarkResponse.ResponseOneofCase.SuccessResponse);

        var (teacherMark, dbTeacherId) = await GetMarkAsync(submittedHomeworkId);
        teacherMark.Should().Be(expectedMark);
        dbTeacherId.Should().Be(teacherId);
    }

    [Fact]
    public async Task V1CorrectSubmittedHomeworkMark_SubmittedHomeworkNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var request = new V1CorrectSubmittedHomeworkMarkRequest
        {
            SubmittedHomeworkId = _fixture.Create<long>(),
            TeacherId = _fixture.Create<long>(),
            TeacherMark = 50
        };

        // Act
        var response = await CorrectSubmittedHomeworkMarkClient.V1CorrectSubmittedHomeworkMarkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CorrectSubmittedHomeworkMarkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
    }

    [Fact]
    public async Task V1CorrectSubmittedHomeworkMark_TeacherDoesNotExists_ShouldBeOtherErrorPermissionDenied()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var otherTeacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Confirmation);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);

        await AddTeacherInDbAsync(teacherId);
        await AddTeacherInDbAsync(otherTeacherId);
        await AddStudentInDbAsync(studentId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);
        await AddSubmittedHomeworkMarkInDbAsync(submittedHomeworkId);

        var request = new V1CorrectSubmittedHomeworkMarkRequest
        {
            SubmittedHomeworkId = submittedHomeworkId,
            TeacherId = otherTeacherId,
            TeacherMark = 50
        };

        // Act
        var response = await CorrectSubmittedHomeworkMarkClient.V1CorrectSubmittedHomeworkMarkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CorrectSubmittedHomeworkMarkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.PermissionDenied);
    }

    [Theory]
    [InlineData(HomeworkStatusModel.Draft)]
    [InlineData(HomeworkStatusModel.Published)]
    [InlineData(HomeworkStatusModel.Reviewing)]
    [InlineData(HomeworkStatusModel.Finished)]
    public async Task V1CorrectSubmittedHomeworkMark_IncorrectHomeworkStatus_ShouldBeOtherErrorConflict(HomeworkStatusModel homeworkStatus)
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, homeworkStatus);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);
        await AddSubmittedHomeworkMarkInDbAsync(submittedHomeworkId);

        var request = new V1CorrectSubmittedHomeworkMarkRequest
        {
            SubmittedHomeworkId = submittedHomeworkId,
            TeacherId = teacherId,
            TeacherMark = 50
        };

        // Act
        var response = await CorrectSubmittedHomeworkMarkClient.V1CorrectSubmittedHomeworkMarkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CorrectSubmittedHomeworkMarkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.Conflict);
    }

    [Fact]
    public async Task V1CorrectSubmittedHomeworkMark_MarkRecordNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Confirmation);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        var request = new V1CorrectSubmittedHomeworkMarkRequest
        {
            SubmittedHomeworkId = submittedHomeworkId,
            TeacherId = teacherId,
            TeacherMark = 50
        };

        // Act
        var response = await CorrectSubmittedHomeworkMarkClient.V1CorrectSubmittedHomeworkMarkAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CorrectSubmittedHomeworkMarkResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1CorrectSubmittedHomeworkMark_NotPositiveSubmittedHomeworkId_ShouldReturnInvalidArgument(long submittedHomeworkId)
    {
        // Arrange
        var request = new V1CorrectSubmittedHomeworkMarkRequest
        {
            SubmittedHomeworkId = submittedHomeworkId,
            TeacherId = _fixture.Create<long>(),
            TeacherMark = 50
        };

        // Act
        var act = async () => await CorrectSubmittedHomeworkMarkClient.V1CorrectSubmittedHomeworkMarkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.SubmittedHomeworkId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1CorrectSubmittedHomeworkMark_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = new V1CorrectSubmittedHomeworkMarkRequest
        {
            SubmittedHomeworkId = _fixture.Create<long>(),
            TeacherId = teacherId,
            TeacherMark = 50
        };

        // Act
        var act = async () => await CorrectSubmittedHomeworkMarkClient.V1CorrectSubmittedHomeworkMarkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task V1CorrectSubmittedHomeworkMark_MarkOutOfRange_ShouldReturnInvalidArgument(int teacherMark)
    {
        // Arrange
        var request = new V1CorrectSubmittedHomeworkMarkRequest
        {
            SubmittedHomeworkId = _fixture.Create<long>(),
            TeacherId = _fixture.Create<long>(),
            TeacherMark = teacherMark
        };

        // Act
        var act = async () => await CorrectSubmittedHomeworkMarkClient.V1CorrectSubmittedHomeworkMarkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherMark));
    }

    private async Task<(int? TeacherMark, long? TeacherId)> GetMarkAsync(long submittedHomeworkId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        var row = await connection.QuerySingleOrDefaultAsync<(int? TeacherMark, long? TeacherId)>(
            "select teacher_mark, teacher_id from submitted_homework_marks where submitted_homework_id = @submittedHomeworkId",
            new { submittedHomeworkId });
        return row;
    }
}
