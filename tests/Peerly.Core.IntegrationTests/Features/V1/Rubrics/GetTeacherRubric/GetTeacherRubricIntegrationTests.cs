using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Features.V1.Rubrics.GetTeacherRubric.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Rubrics.GetTeacherRubric;

public sealed class GetTeacherRubricIntegrationTests : RubricIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public GetTeacherRubricIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    private GetTeacherRubricGrpcClient GetTeacherRubricClient => Fixture.GetTeacherRubricClient;

    [Fact]
    public async Task V1GetTeacherRubric_RubricExistsAndOwnedByTeacher_ShouldReturnRubricAndCriteria()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        await AddTeacherInDbAsync(teacherId);

        var rubricId = await AddRubricInDbAsync(teacherId, "My Rubric");
        await AddRubricCriterionInDbAsync(rubricId, name: "Quality", maxScore: 60, position: 1);
        await AddRubricCriterionInDbAsync(rubricId, name: "Completeness", maxScore: 40, position: 2);

        var request = new V1GetTeacherRubricRequest { RubricId = rubricId, TeacherId = teacherId };

        // Act
        var response = await GetTeacherRubricClient.V1GetTeacherRubricAsync(request);

        // Assert
        response.Rubric.Should().NotBeNull();
        response.Rubric.Id.Should().Be(rubricId);
        response.Rubric.Name.Should().Be("My Rubric");
        response.Rubric.TeacherId.Should().Be(teacherId);
        response.Criteria.Should().HaveCount(2);
    }

    [Fact]
    public async Task V1GetTeacherRubric_RubricNotFound_ShouldThrowNotFound()
    {
        // Arrange
        var request = new V1GetTeacherRubricRequest
        {
            RubricId = _fixture.Create<long>(),
            TeacherId = _fixture.Create<long>()
        };

        // Act
        var act = async () => await GetTeacherRubricClient.V1GetTeacherRubricAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task V1GetTeacherRubric_RubricBelongsToAnotherTeacher_ShouldThrowNotFound()
    {
        // Arrange
        var ownerTeacherId = _fixture.Create<long>();
        var requestingTeacherId = ownerTeacherId + 1;
        await AddTeacherInDbAsync(ownerTeacherId);
        await AddTeacherInDbAsync(requestingTeacherId);

        var rubricId = await AddRubricInDbAsync(ownerTeacherId);

        var request = new V1GetTeacherRubricRequest { RubricId = rubricId, TeacherId = requestingTeacherId };

        // Act
        var act = async () => await GetTeacherRubricClient.V1GetTeacherRubricAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1GetTeacherRubric_NotPositiveRubricId_ShouldReturnInvalidArgument(long rubricId)
    {
        // Arrange
        var request = new V1GetTeacherRubricRequest { RubricId = rubricId, TeacherId = 1 };

        // Act
        var act = async () => await GetTeacherRubricClient.V1GetTeacherRubricAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.RubricId));
    }
}
