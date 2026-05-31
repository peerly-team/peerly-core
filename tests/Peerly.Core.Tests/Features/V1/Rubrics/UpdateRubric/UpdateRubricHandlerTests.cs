using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OneOf.Types;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.UpdateRubric;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.Shared.Models;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Rubrics;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Rubrics.UpdateRubric;

public sealed class UpdateRubricHandlerTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICommandValidator<UpdateRubricCommand, Success>> _validatorMock = new();
    private readonly Mock<IClock> _clockMock = new();

    private readonly Fixture _fixture = new();
    private readonly UpdateRubricHandler _handler;

    public UpdateRubricHandlerTests()
    {
        var unitOfWorkFactoryMock = SetupUnitOfWorkFactory();
        SetupOperationSet();
        _handler = new UpdateRubricHandler(unitOfWorkFactoryMock, _validatorMock.Object, _clockMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultSuccess_ShouldUpdateRubricAndReplaceCriteria()
    {
        // Arrange
        var command = _fixture.Build<UpdateRubricCommand>()
            .With(c => c.Criteria, new[]
            {
                _fixture.Create<RubricCriterionInput>()
            })
            .Create();

        var creationTime = _fixture.Create<DateTimeOffset>();
        _clockMock
            .Setup(clock => clock.GetCurrentTime())
            .Returns(creationTime);

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok);

        var rubricRepositoryMock = new Mock<IRubricRepository>();
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.RubricRepository)
            .Returns(rubricRepositoryMock.Object);

        var updateBuilderMock = new Mock<IUpdateBuilder<RubricUpdateItem>>();
        updateBuilderMock
            .Setup(builder => builder.Set(
                It.Is<Expression<Func<RubricUpdateItem, string>>>(expr => IsNameExpression(expr)),
                command.Name))
            .Returns(updateBuilderMock.Object);

        rubricRepositoryMock
            .Setup(repo => repo.UpdateAsync(
                command.RubricId,
                It.IsAny<Action<IUpdateBuilder<RubricUpdateItem>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<RubricId, Action<IUpdateBuilder<RubricUpdateItem>>, CancellationToken>(
                (_, configureUpdate, _) => configureUpdate(updateBuilderMock.Object))
            .ReturnsAsync(true);

        var rubricCriterionRepositoryMock = new Mock<IRubricCriterionRepository>();
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.RubricCriterionRepository)
            .Returns(rubricCriterionRepositoryMock.Object);

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT0.Should().BeTrue();
        rubricCriterionRepositoryMock.Verify(
            repo => repo.DeleteByRubricIdAsync(
                command.RubricId,
                It.IsAny<CancellationToken>()),
            Times.Once);
        rubricCriterionRepositoryMock.Verify(
            repo => repo.BatchAddAsync(
                It.Is<IReadOnlyCollection<RubricCriterionAddItem>>(items => items.Count == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultOtherError_ShouldBeOtherError()
    {
        // Arrange
        var command = _fixture.Create<UpdateRubricCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtherError.NotFound(RubricErrors.RubricNotFound));

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT2.Should().BeTrue();
        commandResponse.AsT2.Type.Should().Be(ErrorType.NotFound);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.RubricRepository.UpdateAsync(
                It.IsAny<RubricId>(),
                It.IsAny<Action<IUpdateBuilder<RubricUpdateItem>>>(),
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

    private static bool IsNameExpression(Expression<Func<RubricUpdateItem, string>> expression)
    {
        return expression.Body is MemberExpression { Member.Name: nameof(RubricUpdateItem.Name) };
    }
}
