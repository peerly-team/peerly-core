using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Features.V1.Rubrics.GetStudentRubric.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Rubrics.GetStudentRubric;

public sealed class GetStudentRubricIntegrationTests : RubricIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public GetStudentRubricIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    private GetStudentRubricGrpcClient GetStudentRubricClient => Fixture.GetStudentRubricClient;

    [Fact]
    public async Task V1GetStudentRubric_RubricExists_ShouldReturnCriteria()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var studentId = _fixture.Create<long>();
        await AddTeacherInDbAsync(teacherId);
        await AddStudentInDbAsync(studentId);

        var rubricId = await AddRubricInDbAsync(teacherId);
        await AddRubricCriterionInDbAsync(rubricId, name: "Quality", maxScore: 60, position: 1);
        await AddRubricCriterionInDbAsync(rubricId, name: "Completeness", maxScore: 40, position: 2);

        var request = new V1GetStudentRubricRequest { RubricId = rubricId, StudentId = studentId };

        // Act
        var response = await GetStudentRubricClient.V1GetStudentRubricAsync(request);

        // Assert
        response.Criteria.Should().HaveCount(2);
    }

    [Fact]
    public async Task V1GetStudentRubric_RubricNotFound_ShouldThrowNotFound()
    {
        // Arrange
        var request = new V1GetStudentRubricRequest
        {
            RubricId = _fixture.Create<long>(),
            StudentId = _fixture.Create<long>()
        };

        // Act
        var act = async () => await GetStudentRubricClient.V1GetStudentRubricAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetStudentRubric_NotPositiveRubricId_ShouldReturnInvalidArgument(long rubricId)
    {
        // Arrange
        var request = new V1GetStudentRubricRequest { RubricId = rubricId, StudentId = 1 };

        // Act
        var act = async () => await GetStudentRubricClient.V1GetStudentRubricAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.RubricId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetStudentRubric_NotPositiveStudentId_ShouldReturnInvalidArgument(long studentId)
    {
        // Arrange
        var request = new V1GetStudentRubricRequest { RubricId = 1, StudentId = studentId };

        // Act
        var act = async () => await GetStudentRubricClient.V1GetStudentRubricAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.StudentId));
    }
}
