using System;
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
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CorrectSubmittedHomeworkMark;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.CorrectSubmittedHomeworkMark;

public sealed class CorrectSubmittedHomeworkMarkHandlerTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IClock> _clockMock = new();
    private readonly Mock<ICommandValidator<CorrectSubmittedHomeworkMarkCommand, Success>> _validatorMock = new();
    private readonly Fixture _fixture = new();
    private readonly CorrectSubmittedHomeworkMarkHandler _handler;
    private readonly DateTimeOffset _currentTime;

    public CorrectSubmittedHomeworkMarkHandlerTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _currentTime = _fixture.Create<DateTimeOffset>();
        _clockMock.Setup(clock => clock.GetCurrentTime()).Returns(_currentTime);
        _handler = new CorrectSubmittedHomeworkMarkHandler(unitOfWorkFactory, _clockMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationSuccess_ShouldUpdateMarkAndReturnSuccess()
    {
        // Arrange
        var command = _fixture.Create<CorrectSubmittedHomeworkMarkCommand>();

        _validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok);

        var updateBuilderMock = new Mock<IUpdateBuilder<SubmittedHomeworkMarkUpdateItem>>();
        updateBuilderMock
            .Setup(b => b.Set(It.IsAny<Expression<Func<SubmittedHomeworkMarkUpdateItem, int?>>>(), command.TeacherMark))
            .Returns(updateBuilderMock.Object);
        updateBuilderMock
            .Setup(b => b.Set(It.IsAny<Expression<Func<SubmittedHomeworkMarkUpdateItem, TeacherId?>>>(), command.TeacherId))
            .Returns(updateBuilderMock.Object);
        updateBuilderMock
            .Setup(b => b.Set(It.IsAny<Expression<Func<SubmittedHomeworkMarkUpdateItem, DateTimeOffset?>>>(), _currentTime))
            .Returns(updateBuilderMock.Object);

        _unitOfWorkMock
            .Setup(uow => uow.SubmittedHomeworkMarkRepository.UpdateAsync(
                command.SubmittedHomeworkId,
                It.IsAny<Action<IUpdateBuilder<SubmittedHomeworkMarkUpdateItem>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<SubmittedHomeworkId, Action<IUpdateBuilder<SubmittedHomeworkMarkUpdateItem>>, CancellationToken>(
                (_, configure, _) => configure(updateBuilderMock.Object))
            .ReturnsAsync(true);

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT0.Should().BeTrue();
        _unitOfWorkMock.Verify(
            uow => uow.SubmittedHomeworkMarkRepository.UpdateAsync(
                command.SubmittedHomeworkId,
                It.IsAny<Action<IUpdateBuilder<SubmittedHomeworkMarkUpdateItem>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        updateBuilderMock.Verify(
            b => b.Set(
                It.Is<Expression<Func<SubmittedHomeworkMarkUpdateItem, int?>>>(e => IsPropertyExpression(e, nameof(SubmittedHomeworkMarkUpdateItem.TeacherMark))),
                command.TeacherMark),
            Times.Once);
        updateBuilderMock.Verify(
            b => b.Set(
                It.Is<Expression<Func<SubmittedHomeworkMarkUpdateItem, long?>>>(e => IsPropertyExpression(e, nameof(SubmittedHomeworkMarkUpdateItem.TeacherId))),
                (long)command.TeacherId),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationOtherError_ShouldReturnOtherError()
    {
        // Arrange
        var command = _fixture.Create<CorrectSubmittedHomeworkMarkCommand>();

        _validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtherError.PermissionDenied());

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT2.Should().BeTrue();
        response.AsT2.Type.Should().Be(ErrorType.PermissionDenied);
        _unitOfWorkMock.Verify(
            uow => uow.SubmittedHomeworkMarkRepository.UpdateAsync(
                It.IsAny<SubmittedHomeworkId>(),
                It.IsAny<Action<IUpdateBuilder<SubmittedHomeworkMarkUpdateItem>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_MarkRecordNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var command = _fixture.Create<CorrectSubmittedHomeworkMarkCommand>();

        _validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok);

        _unitOfWorkMock
            .Setup(uow => uow.SubmittedHomeworkMarkRepository.UpdateAsync(
                command.SubmittedHomeworkId,
                It.IsAny<Action<IUpdateBuilder<SubmittedHomeworkMarkUpdateItem>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT2.Should().BeTrue();
        response.AsT2.Type.Should().Be(ErrorType.NotFound);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var factoryMock = new Mock<ICommonUnitOfWorkFactory>();
        factoryMock
            .Setup(f => f.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);
        return factoryMock.Object;
    }

    private static bool IsPropertyExpression<TObject, TProperty>(Expression<Func<TObject, TProperty>> expression, string propertyName)
    {
        return expression.Body is MemberExpression { Member.Name: var name } && name == propertyName;
    }
}
