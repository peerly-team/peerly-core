using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Users.UpdateTeacher;

public sealed class UpdateTeacherIntegrationTests : UpdateTeacherIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public UpdateTeacherIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1UpdateTeacher_TeacherExists_ShouldUpdateName()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var newName = _fixture.Create<string>();
        await AddTeacherInDbAsync(teacherId);

        var request = new V1UpdateTeacherRequest
        {
            TeacherId = teacherId,
            Name = newName
        };

        // Act
        var response = await UpdateTeacherClient.V1UpdateTeacherAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateTeacherResponse.ResponseOneofCase.SuccessResponse);

        var updatedName = await GetTeacherNameAsync(teacherId);
        updatedName.Should().Be(newName);
    }

    [Fact]
    public async Task V1UpdateTeacher_TeacherNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var request = new V1UpdateTeacherRequest
        {
            TeacherId = teacherId,
            Name = _fixture.Create<string>()
        };

        // Act
        var response = await UpdateTeacherClient.V1UpdateTeacherAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateTeacherResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1UpdateTeacher_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = _fixture.Build<V1UpdateTeacherRequest>()
            .With(result => result.TeacherId, teacherId)
            .Create();

        // Act
        var act = async () => await UpdateTeacherClient.V1UpdateTeacherAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }

    [Fact]
    public async Task V1UpdateTeacher_EmptyName_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1UpdateTeacherRequest>()
            .With(result => result.Name, string.Empty)
            .Create();

        // Act
        var act = async () => await UpdateTeacherClient.V1UpdateTeacherAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Name));
    }

    private async Task<string> GetTeacherNameAsync(long teacherId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.QuerySingleAsync<string>(
            "select name from teachers where id = @teacherId",
            new { teacherId });
    }
}
