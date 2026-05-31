using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Features.V1.Rubrics.UpdateRubric.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using HomeworkStatusModel = Peerly.Core.Models.Homeworks.HomeworkStatus;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Rubrics.UpdateRubric;

public sealed class UpdateRubricIntegrationTests : RubricIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public UpdateRubricIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    private UpdateRubricGrpcClient UpdateRubricClient => Fixture.UpdateRubricClient;

    [Fact]
    public async Task V1UpdateRubric_RubricExistsAndNotPublished_ShouldUpdateNameAndReplaceCriteria()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        await AddTeacherInDbAsync(teacherId);

        var rubricId = await AddRubricInDbAsync(teacherId, "Old Name");
        await AddRubricCriterionInDbAsync(rubricId);
        await AddRubricCriterionInDbAsync(rubricId, name: "Second", position: 2);

        var request = new V1UpdateRubricRequest
        {
            RubricId = rubricId,
            TeacherId = teacherId,
            Name = "New Name"
        };
        request.Criteria.Add(new RubricCriterionInput
        {
            Name = "Single criterion",
            MaxScore = 100,
            CommentRequired = false,
            Position = 1
        });

        // Act
        var response = await UpdateRubricClient.V1UpdateRubricAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateRubricResponse.ResponseOneofCase.SuccessResponse);

        var rubric = await GetRubricAsync(rubricId);
        rubric.Name.Should().Be("New Name");

        var criteriaCount = await GetRubricCriteriaCountAsync(rubricId);
        criteriaCount.Should().Be(1);
    }

    [Fact]
    public async Task V1UpdateRubric_RubricNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var request = new V1UpdateRubricRequest
        {
            RubricId = _fixture.Create<long>(),
            TeacherId = _fixture.Create<long>(),
            Name = "Name"
        };
        request.Criteria.Add(new RubricCriterionInput { Name = "C", MaxScore = 10, Position = 1 });

        // Act
        var response = await UpdateRubricClient.V1UpdateRubricAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateRubricResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
    }

    [Fact]
    public async Task V1UpdateRubric_RubricBelongsToAnotherTeacher_ShouldBeOtherErrorPermissionDenied()
    {
        // Arrange
        var ownerTeacherId = _fixture.Create<long>();
        var requestingTeacherId = ownerTeacherId + 1;
        await AddTeacherInDbAsync(ownerTeacherId);
        await AddTeacherInDbAsync(requestingTeacherId);

        var rubricId = await AddRubricInDbAsync(ownerTeacherId);

        var request = new V1UpdateRubricRequest
        {
            RubricId = rubricId,
            TeacherId = requestingTeacherId,
            Name = "New Name"
        };
        request.Criteria.Add(new RubricCriterionInput { Name = "C", MaxScore = 10, Position = 1 });

        // Act
        var response = await UpdateRubricClient.V1UpdateRubricAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateRubricResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.PermissionDenied);

        var rubric = await GetRubricAsync(rubricId);
        rubric.Name.Should().NotBe("New Name");
    }

    [Fact]
    public async Task V1UpdateRubric_ReferencedByPublishedHomework_ShouldBeValidationError()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        await AddTeacherInDbAsync(teacherId);

        var rubricId = await AddRubricInDbAsync(teacherId, "Original");
        var courseId = await AddCourseInDbAsync();
        await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published, rubricId: rubricId);

        var request = new V1UpdateRubricRequest
        {
            RubricId = rubricId,
            TeacherId = teacherId,
            Name = "Updated"
        };
        request.Criteria.Add(new RubricCriterionInput { Name = "C", MaxScore = 10, Position = 1 });

        // Act
        var response = await UpdateRubricClient.V1UpdateRubricAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1UpdateRubricResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainSingle("Нельзя изменить рубрику, которая используется в опубликованном домашнем задании");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1UpdateRubric_NotPositiveRubricId_ShouldReturnInvalidArgument(long rubricId)
    {
        // Arrange
        var request = new V1UpdateRubricRequest { RubricId = rubricId, TeacherId = 1, Name = "Name" };
        request.Criteria.Add(new RubricCriterionInput { Name = "C", MaxScore = 10, Position = 1 });

        // Act
        var act = async () => await UpdateRubricClient.V1UpdateRubricAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.RubricId));
    }

    [Fact]
    public async Task V1UpdateRubric_DuplicateCriterionPositions_ShouldReturnInvalidArgument()
    {
        // Arrange
        var positions = new[] { 1, 1, 2 };
        var request = new V1UpdateRubricRequest
        {
            RubricId = 1,
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
        var act = async () => await UpdateRubricClient.V1UpdateRubricAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Criteria));
    }
}
