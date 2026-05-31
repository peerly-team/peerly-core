using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Features.V1.Rubrics.DeleteRubric.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using HomeworkStatusModel = Peerly.Core.Models.Homeworks.HomeworkStatus;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Rubrics.DeleteRubric;

public sealed class DeleteRubricIntegrationTests : RubricIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public DeleteRubricIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    private DeleteRubricGrpcClient DeleteRubricClient => Fixture.DeleteRubricClient;

    [Fact]
    public async Task V1DeleteRubric_RubricExistsAndNotReferenced_ShouldDeleteRubricAndCriteria()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        await AddTeacherInDbAsync(teacherId);

        var rubricId = await AddRubricInDbAsync(teacherId);
        await AddRubricCriterionInDbAsync(rubricId);

        var request = new V1DeleteRubricRequest { RubricId = rubricId, TeacherId = teacherId };

        // Act
        var response = await DeleteRubricClient.V1DeleteRubricAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteRubricResponse.ResponseOneofCase.SuccessResponse);

        var exists = await RubricExistsAsync(rubricId);
        exists.Should().BeFalse();

        var criteriaCount = await GetRubricCriteriaCountAsync(rubricId);
        criteriaCount.Should().Be(0);
    }

    [Fact]
    public async Task V1DeleteRubric_RubricNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var request = new V1DeleteRubricRequest
        {
            RubricId = _fixture.Create<long>(),
            TeacherId = _fixture.Create<long>()
        };

        // Act
        var response = await DeleteRubricClient.V1DeleteRubricAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteRubricResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
        response.OtherError.Message.Should().Be("Рубрика не найдена");
    }

    [Fact]
    public async Task V1DeleteRubric_RubricBelongsToAnotherTeacher_ShouldBeOtherErrorPermissionDenied()
    {
        // Arrange
        var ownerTeacherId = _fixture.Create<long>();
        var requestingTeacherId = ownerTeacherId + 1;
        await AddTeacherInDbAsync(ownerTeacherId);
        await AddTeacherInDbAsync(requestingTeacherId);

        var rubricId = await AddRubricInDbAsync(ownerTeacherId);

        var request = new V1DeleteRubricRequest { RubricId = rubricId, TeacherId = requestingTeacherId };

        // Act
        var response = await DeleteRubricClient.V1DeleteRubricAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteRubricResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.PermissionDenied);
    }

    [Fact]
    public async Task V1DeleteRubric_RubricReferencedByHomework_ShouldBeValidationError()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        await AddTeacherInDbAsync(teacherId);

        var rubricId = await AddRubricInDbAsync(teacherId);
        var courseId = await AddCourseInDbAsync();
        await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Draft, rubricId: rubricId);

        var request = new V1DeleteRubricRequest { RubricId = rubricId, TeacherId = teacherId };

        // Act
        var response = await DeleteRubricClient.V1DeleteRubricAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1DeleteRubricResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainSingle("Нельзя удалить рубрику, которая используется в домашнем задании");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1DeleteRubric_NotPositiveRubricId_ShouldReturnInvalidArgument(long rubricId)
    {
        // Arrange
        var request = new V1DeleteRubricRequest { RubricId = rubricId, TeacherId = 1 };

        // Act
        var act = async () => await DeleteRubricClient.V1DeleteRubricAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.RubricId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1DeleteRubric_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = new V1DeleteRubricRequest { RubricId = 1, TeacherId = teacherId };

        // Act
        var act = async () => await DeleteRubricClient.V1DeleteRubricAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }
}
