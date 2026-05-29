using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.DeleteRubric;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Rubrics;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Rubrics.DeleteRubric;

public sealed class DeleteRubricValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();

    private readonly Fixture _fixture = new();
    private readonly DeleteRubricValidator _validator;

    public DeleteRubricValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new DeleteRubricValidator(unitOfWorkFactory);
    }

    [Fact]
    public async Task ValidateAsync_RubricExistsAndOwnedByTeacherAndNotReferenced_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Create<DeleteRubricCommand>();

        var rubric = _fixture.Build<Rubric>()
            .With(r => r.Id, command.RubricId)
            .With(r => r.TeacherId, command.TeacherId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyRubricRepository.GetAsync(command.RubricId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rubric);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyHomeworkRepository.ExistsByRubricIdAsync(command.RubricId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_RubricNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<DeleteRubricCommand>();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyRubricRepository.GetAsync(command.RubricId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Rubric?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(RubricErrors.RubricNotFound);
    }

    [Fact]
    public async Task ValidateAsync_RubricBelongsToAnotherTeacher_ShouldBeOtherErrorPermissionDenied()
    {
        // Arrange
        var command = _fixture.Create<DeleteRubricCommand>();

        var rubric = _fixture.Build<Rubric>()
            .With(r => r.Id, command.RubricId)
            .With(r => r.TeacherId, _fixture.Create<TeacherId>())
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyRubricRepository.GetAsync(command.RubricId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rubric);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.PermissionDenied);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyHomeworkRepository.ExistsByRubricIdAsync(
                It.IsAny<RubricId>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_RubricReferencedByHomework_ShouldBeValidationError()
    {
        // Arrange
        var command = _fixture.Create<DeleteRubricCommand>();

        var rubric = _fixture.Build<Rubric>()
            .With(r => r.Id, command.RubricId)
            .With(r => r.TeacherId, command.TeacherId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyRubricRepository.GetAsync(command.RubricId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rubric);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyHomeworkRepository.ExistsByRubricIdAsync(command.RubricId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Errors.Should().NotBeNull().And.ContainSingle(RubricErrors.RubricReferencedByHomework.Value);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var unitOfWorkFactoryMock = new Mock<ICommonUnitOfWorkFactory>();
        unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        return unitOfWorkFactoryMock.Object;
    }
}
