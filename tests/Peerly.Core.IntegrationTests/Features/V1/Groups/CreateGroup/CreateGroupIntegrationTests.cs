using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Groups.CreateGroup;

public sealed class CreateGroupIntegrationTests : CreateGroupIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public CreateGroupIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1CreateGroup_CourseTeacherAndCourseExists_ShouldCreateGroup()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var request = _fixture.Build<V1CreateGroupRequest>()
            .With(result => result.CourseId, courseId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        // Act
        var response = await CreateGroupClient.V1CreateGroupAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateGroupResponse.ResponseOneofCase.SuccessResponse);
        response.SuccessResponse.GroupId.Should().BePositive();

        var group = await GetCreatedGroupAsync(response.SuccessResponse.GroupId);
        group.Should().BeEquivalentTo((request.CourseId, request.Name));
    }

    [Fact]
    public async Task V1CreateGroup_CourseTeacherNotFound_ShouldBeOtherErrorPermissionDenied()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var courseId = await AddCourseInDbAsync();
        var request = _fixture.Build<V1CreateGroupRequest>()
            .With(result => result.CourseId, courseId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);

        // Act
        var response = await CreateGroupClient.V1CreateGroupAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateGroupResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.PermissionDenied);
        response.OtherError.Message.Should().BeNull();

        var groupExists = await IsGroupExistsAsync(request.Name);
        groupExists.Should().BeFalse();
    }

    [Fact]
    public async Task V1CreateGroup_CourseNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var courseId = _fixture.Create<long>();
        var request = _fixture.Build<V1CreateGroupRequest>()
            .With(result => result.CourseId, courseId)
            .With(result => result.TeacherId, teacherId)
            .Create();

        await AddTeacherInDbAsync(teacherId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        // Act
        var response = await CreateGroupClient.V1CreateGroupAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateGroupResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().BeNull();

        var groupExists = await IsGroupExistsAsync(request.Name);
        groupExists.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1CreateGroup_NotPositiveCourseId_ShouldReturnInvalidArgument(long courseId)
    {
        // Arrange
        var request = _fixture.Build<V1CreateGroupRequest>()
            .With(result => result.CourseId, courseId)
            .Create();

        // Act
        var act = async () => await CreateGroupClient.V1CreateGroupAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.CourseId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1CreateGroup_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = _fixture.Build<V1CreateGroupRequest>()
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var act = async () => await CreateGroupClient.V1CreateGroupAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }

    [Fact]
    public async Task V1CreateGroup_EmptyName_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1CreateGroupRequest>()
            .With(result => result.Name, string.Empty)
            .Create();

        // Act
        var act = async () => await CreateGroupClient.V1CreateGroupAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Name));
    }

    private async Task<(long CourseId, string Name)> GetCreatedGroupAsync(long groupId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        var row = await connection.QuerySingleAsync(
            "select course_id, name from groups where id = @groupId",
            new { groupId });
        return (row.course_id, row.name);
    }

    private async Task<bool> IsGroupExistsAsync(string name)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(
            "select exists(select from groups where name = @name)",
            new { name });
    }
}
