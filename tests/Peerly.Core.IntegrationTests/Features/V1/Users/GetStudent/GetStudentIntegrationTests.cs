using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Users.GetStudent;

public sealed class GetStudentIntegrationTests : GetStudentIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public GetStudentIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1GetStudent_StudentExists_ShouldReturnStudentInfo()
    {
        // Arrange
        var studentId = _fixture.Create<long>();
        var studentEmail = $"student-{studentId}@peerly.test";
        var studentName = $"Student {studentId}";
        await AddStudentInDbAsync(studentId, studentEmail, studentName);

        var request = new V1GetStudentRequest { StudentId = studentId };

        // Act
        var response = await GetStudentClient.V1GetStudentAsync(request);

        // Assert
        response.StudentInfo.StudentId.Should().Be(studentId);
        response.StudentInfo.Email.Should().Be(studentEmail);
        response.StudentInfo.Name.Should().Be(studentName);
    }

    [Fact]
    public async Task V1GetStudent_StudentNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var studentId = _fixture.Create<long>();
        var request = new V1GetStudentRequest { StudentId = studentId };

        // Act
        var act = async () => await GetStudentClient.V1GetStudentAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetStudent_NotPositiveStudentId_ShouldReturnInvalidArgument(long studentId)
    {
        // Arrange
        var request = new V1GetStudentRequest { StudentId = studentId };

        // Act
        var act = async () => await GetStudentClient.V1GetStudentAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StudentId));
    }
}
