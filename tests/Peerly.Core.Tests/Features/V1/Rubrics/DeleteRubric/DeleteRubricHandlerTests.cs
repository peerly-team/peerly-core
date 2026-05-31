using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OneOf.Types;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.DeleteRubric;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Rubrics.DeleteRubric;

public sealed class DeleteRubricHandlerTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICommandValidator<DeleteRubricCommand, Success>> _validatorMock = new();

    private readonly Fixture _fixture = new();
    private readonly DeleteRubricHandler _handler;

    public DeleteRubricHandlerTests()
    {
        var unitOfWorkFactoryMock = SetupUnitOfWorkFactory();
        SetupOperationSet();
        _handler = new DeleteRubricHandler(unitOfWorkFactoryMock, _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultSuccess_ShouldDeleteCriteriaAndRubric()
    {
        // Arrange
        var command = _fixture.Create<DeleteRubricCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok);

        var rubricCriterionRepositoryMock = new Mock<IRubricCriterionRepository>();
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.RubricCriterionRepository)
            .Returns(rubricCriterionRepositoryMock.Object);

        var rubricRepositoryMock = new Mock<IRubricRepository>();
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.RubricRepository)
            .Returns(rubricRepositoryMock.Object);

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT0.Should().BeTrue();
        rubricCriterionRepositoryMock.Verify(
            repo => repo.DeleteByRubricIdAsync(
                command.RubricId,
                It.IsAny<CancellationToken>()),
            Times.Once);
        rubricRepositoryMock.Verify(
            repo => repo.DeleteAsync(
                command.RubricId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultOtherError_ShouldBeOtherError()
    {
        // Arrange
        var command = _fixture.Create<DeleteRubricCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtherError.NotFound(RubricErrors.RubricNotFound));

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT2.Should().BeTrue();
        commandResponse.AsT2.Type.Should().Be(ErrorType.NotFound);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.RubricRepository.DeleteAsync(
                It.IsAny<RubricId>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultValidationError_ShouldBeValidationError()
    {
        // Arrange
        var command = _fixture.Create<DeleteRubricCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationError.From(RubricErrors.RubricReferencedByHomework));

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT1.Should().BeTrue();
        commandResponse.AsT1.Errors.Should().NotBeNull().And.ContainSingle(RubricErrors.RubricReferencedByHomework.Value);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.RubricRepository.DeleteAsync(
                It.IsAny<RubricId>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var unitOfWorkFactoryMock = new Mock<ICommonUnitOfWorkFactory>();
        unitOfWorkFactoryMock
            .Setup(factory => factory.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        return unitOfWorkFactoryMock.Object;
    }

    private void SetupOperationSet()
    {
        var operationSetMock = new Mock<IOperationSet>();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.StartOperationSet(It.IsAny<CancellationToken>()))
            .ReturnsAsync(operationSetMock.Object);
    }
}
