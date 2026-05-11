using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;
using HomeworkStatusModel = Peerly.Core.Models.Homeworks.HomeworkStatus;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.GetSubmittedHomework;

public sealed class GetSubmittedHomeworkIntegrationTests : GetSubmittedHomeworkIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public GetSubmittedHomeworkIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1GetSubmittedHomework_SubmittedHomeworkBelongsToStudent_ShouldReturnSubmittedHomework()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var ownerStudentId = _fixture.Create<long>();
        var reviewerStudentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(reviewerStudentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Finished);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId);
        var file = await AddFileInDbAsync(_fixture.Create<string>(), 1024);
        await AddSubmittedHomeworkFileInDbAsync(submittedHomeworkId, file.Id);
        var submittedReviewId = await AddSubmittedReviewInDbAsync(submittedHomeworkId, reviewerStudentId, mark: 81);
        await AddSubmittedHomeworkMarkInDbAsync(submittedHomeworkId, reviewersMark: 81);

        var request = _fixture.Build<V1GetSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, ownerStudentId)
            .Create();

        // Act
        var response = await GetSubmittedHomeworkClient.V1GetSubmittedHomeworkAsync(request);

        // Assert
        response.SubmittedHomework.Id.Should().Be(submittedHomeworkId);
        response.SubmittedHomework.Comment.Should().Be("Test comment");
        response.SubmittedHomework.Files.Should().ContainSingle().Which
            .Should().BeEquivalentTo(new { file.Id, file.Name, file.Size });
        response.SubmittedReviews.Should().ContainSingle().Which
            .Should().BeEquivalentTo(new { Id = submittedReviewId, Mark = 81, Comment = "Review comment" });
        response.FinalMark.Should().Be(81);
    }

    [Fact]
    public async Task V1GetSubmittedHomework_TeacherMarkExists_ShouldReturnTeacherMarkAsFinalMark()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Finished);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);
        await AddSubmittedHomeworkMarkInDbAsync(submittedHomeworkId, reviewersMark: 70, teacherMark: 95);

        var request = _fixture.Build<V1GetSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await GetSubmittedHomeworkClient.V1GetSubmittedHomeworkAsync(request);

        // Assert
        response.FinalMark.Should().Be(95);
    }

    [Fact]
    public async Task V1GetSubmittedHomework_MarkNotFound_ShouldNotSetFinalMark()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Finished);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, studentId);

        var request = _fixture.Build<V1GetSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var response = await GetSubmittedHomeworkClient.V1GetSubmittedHomeworkAsync(request);

        // Assert
        response.HasFinalMark.Should().BeFalse();
    }

    [Fact]
    public async Task V1GetSubmittedHomework_HomeworkNotFinished_ShouldHideReviewsAndFinalMark()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var ownerStudentId = _fixture.Create<long>();
        var reviewerStudentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(reviewerStudentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Confirmation);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId);
        var file = await AddFileInDbAsync(_fixture.Create<string>(), 1024);
        await AddSubmittedHomeworkFileInDbAsync(submittedHomeworkId, file.Id);
        await AddSubmittedReviewInDbAsync(submittedHomeworkId, reviewerStudentId, mark: 81);
        await AddSubmittedHomeworkMarkInDbAsync(submittedHomeworkId, reviewersMark: 81);

        var request = _fixture.Build<V1GetSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, ownerStudentId)
            .Create();

        // Act
        var response = await GetSubmittedHomeworkClient.V1GetSubmittedHomeworkAsync(request);

        // Assert
        response.SubmittedHomework.Id.Should().Be(submittedHomeworkId);
        response.SubmittedHomework.Comment.Should().Be("Test comment");
        response.SubmittedHomework.Files.Should().ContainSingle().Which
            .Should().BeEquivalentTo(new { file.Id, file.Name, file.Size });
        response.SubmittedReviews.Should().BeEmpty();
        response.HasFinalMark.Should().BeFalse();
    }

    [Fact]
    public async Task V1GetSubmittedHomework_SubmittedHomeworkNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var request = _fixture.Build<V1GetSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, _fixture.Create<long>())
            .With(result => result.StudentId, _fixture.Create<long>())
            .Create();

        // Act
        var act = async () => await GetSubmittedHomeworkClient.V1GetSubmittedHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task V1GetSubmittedHomework_WrongStudent_ShouldReturnNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var ownerStudentId = _fixture.Create<long>();
        var requestingStudentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(ownerStudentId);
        await AddStudentInDbAsync(requestingStudentId);

        var homeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Confirmation);
        var submittedHomeworkId = await AddSubmittedHomeworkInDbAsync(homeworkId, ownerStudentId);
        var request = _fixture.Build<V1GetSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, requestingStudentId)
            .Create();

        // Act
        var act = async () => await GetSubmittedHomeworkClient.V1GetSubmittedHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetSubmittedHomework_NotPositiveSubmittedHomeworkId_ShouldReturnInvalidArgument(long submittedHomeworkId)
    {
        // Arrange
        var request = _fixture.Build<V1GetSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .With(result => result.StudentId, 1)
            .Create();

        // Act
        var act = async () => await GetSubmittedHomeworkClient.V1GetSubmittedHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.SubmittedHomeworkId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetSubmittedHomework_NotPositiveStudentId_ShouldReturnInvalidArgument(long studentId)
    {
        // Arrange
        var request = _fixture.Build<V1GetSubmittedHomeworkRequest>()
            .With(result => result.SubmittedHomeworkId, 1)
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var act = async () => await GetSubmittedHomeworkClient.V1GetSubmittedHomeworkAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StudentId));
    }
}
