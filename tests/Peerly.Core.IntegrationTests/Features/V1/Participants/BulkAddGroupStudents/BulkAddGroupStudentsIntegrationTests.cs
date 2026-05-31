using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Participants.BulkAddGroupStudents;

public sealed class BulkAddGroupStudentsIntegrationTests : BulkAddGroupStudentsIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public BulkAddGroupStudentsIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1BulkAddGroupStudents_SomeStudentsAddedAndSomeSkipped_ShouldReturnPartialResult()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var addedStudentId = _fixture.Create<long>();
        var alreadyInGroupStudentId = _fixture.Create<long>();
        var missingStudentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);

        await AddTeacherInDbAsync(teacherId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);
        await AddStudentInDbAsync(addedStudentId);
        await AddStudentInDbAsync(alreadyInGroupStudentId);
        await AddGroupStudentInDbAsync(groupId, alreadyInGroupStudentId);

        var request = new V1BulkAddGroupStudentsRequest
        {
            GroupId = groupId,
            TeacherId = teacherId,
            StudentIds = { addedStudentId, alreadyInGroupStudentId, missingStudentId }
        };

        // Act
        var response = await BulkAddGroupStudentsClient.V1BulkAddGroupStudentsAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1BulkAddGroupStudentsResponse.ResponseOneofCase.SuccessResponse);
        response.SuccessResponse.AddedStudentIds.Should().Equal(addedStudentId);
        response.SuccessResponse.SkippedStudentInfos.Should().BeEquivalentTo(
        [
            new V1BulkAddGroupStudentsResponse.Types.SkippedStudentInfo
            {
                StudentId = alreadyInGroupStudentId,
                Reason = V1BulkAddGroupStudentsResponse.Types.SkipReason.AlreadyInGroup
            },
            new V1BulkAddGroupStudentsResponse.Types.SkippedStudentInfo
            {
                StudentId = missingStudentId,
                Reason = V1BulkAddGroupStudentsResponse.Types.SkipReason.NotFound
            }
        ], options => options.WithStrictOrdering());

        var groupStudentIds = await ListGroupStudentIdsAsync(groupId);
        groupStudentIds.Should().BeEquivalentTo([addedStudentId, alreadyInGroupStudentId]);
    }

    [Fact]
    public async Task V1BulkAddGroupStudents_TeacherHasNoCourseAccess_ShouldBeOtherErrorPermissionDenied()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);

        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);

        var request = new V1BulkAddGroupStudentsRequest
        {
            GroupId = groupId,
            TeacherId = teacherId,
            StudentIds = { studentId }
        };

        // Act
        var response = await BulkAddGroupStudentsClient.V1BulkAddGroupStudentsAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1BulkAddGroupStudentsResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.PermissionDenied);

        var groupStudentIds = await ListGroupStudentIdsAsync(groupId);
        groupStudentIds.Should().BeEmpty();
    }

    [Fact]
    public async Task V1BulkAddGroupStudents_DuplicateStudentIds_ShouldReturnInvalidArgument()
    {
        // Arrange
        var studentId = _fixture.Create<long>();
        var request = new V1BulkAddGroupStudentsRequest
        {
            GroupId = _fixture.Create<long>(),
            TeacherId = _fixture.Create<long>(),
            StudentIds = { studentId, studentId }
        };

        // Act
        var act = async () => await BulkAddGroupStudentsClient.V1BulkAddGroupStudentsAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StudentIds));
    }
}
