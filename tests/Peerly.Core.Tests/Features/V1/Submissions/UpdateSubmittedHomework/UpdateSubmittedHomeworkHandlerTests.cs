using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OneOf.Types;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.UpdateSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.UpdateSubmittedHomework;

public sealed class UpdateSubmittedHomeworkHandlerTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ISubmittedHomeworkRepository> _submittedHomeworkRepositoryMock = new();
    private readonly Mock<ICommandValidator<UpdateSubmittedHomeworkCommand, Success>> _validatorMock = new();
    private readonly Fixture _fixture = new();
    private readonly UpdateSubmittedHomeworkHandler _handler;

    public UpdateSubmittedHomeworkHandlerTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _handler = new UpdateSubmittedHomeworkHandler(unitOfWorkFactory, _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationSuccess_ShouldUpdateSubmittedHomeworkComment()
    {
        // Arrange
        var command = _fixture.Create<UpdateSubmittedHomeworkCommand>();
        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok());

        var updateBuilderMock = new Mock<IUpdateBuilder<SubmittedHomeworkUpdateItem>>();
        updateBuilderMock
            .Setup(builder => builder.Set(
                It.Is<Expression<Func<SubmittedHomeworkUpdateItem, string>>>(expression => IsCommentExpression(expression)),
                command.Comment))
            .Returns(updateBuilderMock.Object);

        _submittedHomeworkRepositoryMock
            .Setup(repository => repository.UpdateAsync(
                command.SubmittedHomeworkId,
                It.IsAny<Action<IUpdateBuilder<SubmittedHomeworkUpdateItem>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<SubmittedHomeworkId, Action<IUpdateBuilder<SubmittedHomeworkUpdateItem>>, CancellationToken>(
                (_, configureUpdate, _) => configureUpdate(updateBuilderMock.Object))
            .ReturnsAsync(true);

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT0.Should().BeTrue();
        _submittedHomeworkRepositoryMock.Verify(
            repository => repository.UpdateAsync(
                command.SubmittedHomeworkId,
                It.IsAny<Action<IUpdateBuilder<SubmittedHomeworkUpdateItem>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        updateBuilderMock.Verify(
            builder => builder.Set(
                It.Is<Expression<Func<SubmittedHomeworkUpdateItem, string>>>(expression => IsCommentExpression(expression)),
                command.Comment),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultOtherError_ShouldBeOtherError()
    {
        // Arrange
        var command = _fixture.Create<UpdateSubmittedHomeworkCommand>();
        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtherError.NotFound(SubmittedHomeworkErrors.SubmittedHomeworkNotFound));

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT2.Should().BeTrue();
        response.AsT2.Type.Should().Be(ErrorType.NotFound);
        response.AsT2.Message.Should().Be(SubmittedHomeworkErrors.SubmittedHomeworkNotFound);
        _submittedHomeworkRepositoryMock.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<SubmittedHomeworkId>(),
                It.IsAny<Action<IUpdateBuilder<SubmittedHomeworkUpdateItem>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultValidationError_ShouldBeValidationError()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();
        var command = _fixture.Create<UpdateSubmittedHomeworkCommand>();
        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationError.From(errorMessage));

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT1.Should().BeTrue();
        response.AsT1.Errors.Should().NotBeNull().And.ContainSingle(errorMessage);
        _submittedHomeworkRepositoryMock.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<SubmittedHomeworkId>(),
                It.IsAny<Action<IUpdateBuilder<SubmittedHomeworkUpdateItem>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var unitOfWorkFactoryMock = new Mock<ICommonUnitOfWorkFactory>();
        unitOfWorkFactoryMock
            .Setup(factory => factory.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.SubmittedHomeworkRepository)
            .Returns(_submittedHomeworkRepositoryMock.Object);

        return unitOfWorkFactoryMock.Object;
    }

    private static bool IsCommentExpression(Expression<Func<SubmittedHomeworkUpdateItem, string>> expression)
    {
        return expression.Body is MemberExpression { Member.Name: nameof(SubmittedHomeworkUpdateItem.Comment) };
    }
}
