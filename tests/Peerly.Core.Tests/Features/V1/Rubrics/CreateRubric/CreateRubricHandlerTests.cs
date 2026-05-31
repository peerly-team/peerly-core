using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.CreateRubric;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.Shared.Models;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Rubrics;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Rubrics.CreateRubric;

public sealed class CreateRubricHandlerTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IClock> _clockMock = new();

    private readonly Fixture _fixture = new();
    private readonly CreateRubricHandler _handler;

    public CreateRubricHandlerTests()
    {
        var unitOfWorkFactoryMock = SetupUnitOfWorkFactory();
        SetupOperationSet();
        _handler = new CreateRubricHandler(unitOfWorkFactoryMock, _clockMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidCommand_ShouldCreateRubricAndCriteria()
    {
        // Arrange
        var command = _fixture.Build<CreateRubricCommand>()
            .With(c => c.Criteria,
            [
                _fixture.Create<RubricCriterionInput>(),
                _fixture.Create<RubricCriterionInput>()
            ])
            .Create();

        var creationTime = _fixture.Create<DateTimeOffset>();
        _clockMock
            .Setup(clock => clock.GetCurrentTime())
            .Returns(creationTime);

        var expectedRubricId = _fixture.Create<RubricId>();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.RubricRepository.AddAsync(
                It.Is<RubricAddItem>(item =>
                    item.TeacherId == command.TeacherId &&
                    item.Name == command.Name &&
                    item.CreationTime == creationTime),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRubricId);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.RubricCriterionRepository.BatchAddAsync(
                It.Is<IReadOnlyCollection<RubricCriterionAddItem>>(items => items.Count == 2),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT0.Should().BeTrue();
        commandResponse.AsT0.RubricId.Should().Be(expectedRubricId);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.RubricRepository.AddAsync(
                It.IsAny<RubricAddItem>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.RubricCriterionRepository.BatchAddAsync(
                It.IsAny<IReadOnlyCollection<RubricCriterionAddItem>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
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
