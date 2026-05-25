using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Users.UpdateStudent;

public sealed class UpdateStudentIntegrationTests : UpdateStudentIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public UpdateStudentIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1UpdateStudent_StudentExists_ShouldUpdateName()
    {
        // Arrange
        var studentId = _fixture.Create<long>();
        var newName = _fixture.Create<string>();
        await AddStudentInDbAsync(studentId);

        var request = new V1UpdateStudentRequest
        {
            StudentId = studentId,
            Name = newName
        };

        // Act
        var response = await UpdateStudentClient.V1UpdateStudentAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateStudentResponse.ResponseOneofCase.SuccessResponse);

        var updatedName = await GetStudentNameAsync(studentId);
        updatedName.Should().Be(newName);
    }

    [Fact]
    public async Task V1UpdateStudent_StudentNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var studentId = _fixture.Create<long>();
        var request = new V1UpdateStudentRequest
        {
            StudentId = studentId,
            Name = _fixture.Create<string>()
        };

        // Act
        var response = await UpdateStudentClient.V1UpdateStudentAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateStudentResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1UpdateStudent_NotPositiveStudentId_ShouldReturnInvalidArgument(long studentId)
    {
        // Arrange
        var request = _fixture.Build<V1UpdateStudentRequest>()
            .With(result => result.StudentId, studentId)
            .Create();

        // Act
        var act = async () => await UpdateStudentClient.V1UpdateStudentAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StudentId));
    }

    [Fact]
    public async Task V1UpdateStudent_EmptyName_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1UpdateStudentRequest>()
            .With(result => result.Name, string.Empty)
            .Create();

        // Act
        var act = async () => await UpdateStudentClient.V1UpdateStudentAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Name));
    }

    private async Task<string> GetStudentNameAsync(long studentId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        return await connection.QuerySingleAsync<string>(
            "select name from students where id = @studentId",
            new { studentId });
    }
}
