using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Users.GetTeacher;

public sealed class GetTeacherIntegrationTests : GetTeacherIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public GetTeacherIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1GetTeacher_TeacherExists_ShouldReturnTeacherInfo()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var teacherEmail = $"teacher-{teacherId}@peerly.test";
        var teacherName = $"Teacher {teacherId}";
        await AddTeacherInDbAsync(teacherId, teacherEmail, teacherName);

        var request = new V1GetTeacherRequest { TeacherId = teacherId };

        // Act
        var response = await GetTeacherClient.V1GetTeacherAsync(request);

        // Assert
        response.TeacherInfo.TeacherId.Should().Be(teacherId);
        response.TeacherInfo.Email.Should().Be(teacherEmail);
        response.TeacherInfo.Name.Should().Be(teacherName);
    }

    [Fact]
    public async Task V1GetTeacher_TeacherNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var request = new V1GetTeacherRequest { TeacherId = teacherId };

        // Act
        var act = async () => await GetTeacherClient.V1GetTeacherAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetTeacher_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = new V1GetTeacherRequest { TeacherId = teacherId };

        // Act
        var act = async () => await GetTeacherClient.V1GetTeacherAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }
}
