using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Features.V1.Rubrics.CreateRubric.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Rubrics.CreateRubric;

public sealed class CreateRubricIntegrationTests : RubricIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public CreateRubricIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    private CreateRubricGrpcClient CreateRubricClient => Fixture.CreateRubricClient;

    [Fact]
    public async Task V1CreateRubric_ValidRequest_ShouldCreateRubricWithCriteria()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        await AddTeacherInDbAsync(teacherId);

        var request = new V1CreateRubricRequest
        {
            TeacherId = teacherId,
            Name = "Test Rubric"
        };
        request.Criteria.Add(new RubricCriterionInput
        {
            Name = "Code quality",
            Description = "Code style and readability",
            MaxScore = 50,
            CommentRequired = false,
            Position = 1
        });
        request.Criteria.Add(new RubricCriterionInput
        {
            Name = "Correctness",
            MaxScore = 50,
            CommentRequired = true,
            Position = 2
        });

        // Act
        var response = await CreateRubricClient.V1CreateRubricAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1CreateRubricResponse.ResponseOneofCase.SuccessResponse);
        response.SuccessResponse.RubricId.Should().BeGreaterThan(0);

        var rubric = await GetRubricAsync(response.SuccessResponse.RubricId);
        rubric.Name.Should().Be("Test Rubric");
        rubric.TeacherId.Should().Be(teacherId);

        var criteriaCount = await GetRubricCriteriaCountAsync(response.SuccessResponse.RubricId);
        criteriaCount.Should().Be(2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1CreateRubric_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = new V1CreateRubricRequest
        {
            TeacherId = teacherId,
            Name = "Test Rubric"
        };
        request.Criteria.Add(new RubricCriterionInput
        {
            Name = "Criterion",
            MaxScore = 100,
            Position = 1
        });

        // Act
        var act = async () => await CreateRubricClient.V1CreateRubricAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }

    [Fact]
    public async Task V1CreateRubric_EmptyName_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = new V1CreateRubricRequest
        {
            TeacherId = 1,
            Name = string.Empty
        };
        request.Criteria.Add(new RubricCriterionInput
        {
            Name = "Criterion",
            MaxScore = 100,
            Position = 1
        });

        // Act
        var act = async () => await CreateRubricClient.V1CreateRubricAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Name));
    }

    [Fact]
    public async Task V1CreateRubric_EmptyCriteria_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = new V1CreateRubricRequest
        {
            TeacherId = 1,
            Name = "Test Rubric"
        };

        // Act
        var act = async () => await CreateRubricClient.V1CreateRubricAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Criteria));
    }

    [Fact]
    public async Task V1CreateRubric_DuplicateCriterionPositions_ShouldReturnInvalidArgument()
    {
        // Arrange
        var positions = new[] { 1, 1, 2 };
        var request = new V1CreateRubricRequest
        {
            TeacherId = 1,
            Name = "Test Rubric"
        };

        foreach (var position in positions)
        {
            request.Criteria.Add(new RubricCriterionInput
            {
                Name = "Criterion",
                MaxScore = 100,
                Position = position
            });
        }

        // Act
        var act = async () => await CreateRubricClient.V1CreateRubricAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Criteria));
    }
}
