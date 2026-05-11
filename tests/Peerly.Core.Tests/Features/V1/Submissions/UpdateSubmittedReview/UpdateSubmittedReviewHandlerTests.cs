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
using Peerly.Core.ApplicationServices.Features.V1.Submissions.UpdateSubmittedReview;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.UpdateSubmittedReview;

public sealed class UpdateSubmittedReviewHandlerTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ISubmittedReviewRepository> _submittedReviewRepositoryMock = new();
    private readonly Mock<ICommandValidator<UpdateSubmittedReviewCommand, Success>> _validatorMock = new();
    private readonly Fixture _fixture = new();
    private readonly UpdateSubmittedReviewHandler _handler;

    public UpdateSubmittedReviewHandlerTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _handler = new UpdateSubmittedReviewHandler(unitOfWorkFactory, _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationSuccess_ShouldUpdateSubmittedReview()
    {
        // Arrange
        var command = _fixture.Create<UpdateSubmittedReviewCommand>();
        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok());

        var updateBuilderMock = new Mock<IUpdateBuilder<SubmittedReviewUpdateItem>>();
        updateBuilderMock
            .Setup(builder => builder.Set(It.IsAny<Expression<Func<SubmittedReviewUpdateItem, int>>>(), command.Mark))
            .Returns(updateBuilderMock.Object);
        updateBuilderMock
            .Setup(builder => builder.Set(It.IsAny<Expression<Func<SubmittedReviewUpdateItem, string>>>(), command.Comment))
            .Returns(updateBuilderMock.Object);

        _submittedReviewRepositoryMock
            .Setup(repository => repository.UpdateAsync(
                command.SubmittedReviewId,
                It.IsAny<Action<IUpdateBuilder<SubmittedReviewUpdateItem>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<SubmittedReviewId, Action<IUpdateBuilder<SubmittedReviewUpdateItem>>, CancellationToken>(
                (_, configureUpdate, _) => configureUpdate(updateBuilderMock.Object))
            .ReturnsAsync(true);

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT0.Should().BeTrue();
        _submittedReviewRepositoryMock.Verify(
            repository => repository.UpdateAsync(
                command.SubmittedReviewId,
                It.IsAny<Action<IUpdateBuilder<SubmittedReviewUpdateItem>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        updateBuilderMock.Verify(
            builder => builder.Set(
                It.Is<Expression<Func<SubmittedReviewUpdateItem, int>>>(expression => IsPropertyExpression(expression, nameof(SubmittedReviewUpdateItem.Mark))),
                command.Mark),
            Times.Once);
        updateBuilderMock.Verify(
            builder => builder.Set(
                It.Is<Expression<Func<SubmittedReviewUpdateItem, string>>>(expression => IsPropertyExpression(expression, nameof(SubmittedReviewUpdateItem.Comment))),
                command.Comment),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultOtherError_ShouldBeOtherError()
    {
        // Arrange
        var command = _fixture.Create<UpdateSubmittedReviewCommand>();
        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtherError.NotFound(SubmittedReviewErrors.SubmittedReviewNotFound));

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT2.Should().BeTrue();
        response.AsT2.Type.Should().Be(ErrorType.NotFound);
        response.AsT2.Message.Should().Be(SubmittedReviewErrors.SubmittedReviewNotFound);
        _submittedReviewRepositoryMock.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<SubmittedReviewId>(),
                It.IsAny<Action<IUpdateBuilder<SubmittedReviewUpdateItem>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultValidationError_ShouldBeValidationError()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();
        var command = _fixture.Create<UpdateSubmittedReviewCommand>();
        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationError.From(errorMessage));

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT1.Should().BeTrue();
        response.AsT1.Errors.Should().NotBeNull().And.ContainSingle(errorMessage);
        _submittedReviewRepositoryMock.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<SubmittedReviewId>(),
                It.IsAny<Action<IUpdateBuilder<SubmittedReviewUpdateItem>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var factoryMock = new Mock<ICommonUnitOfWorkFactory>();
        factoryMock
            .Setup(factory => factory.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.SubmittedReviewRepository)
            .Returns(_submittedReviewRepositoryMock.Object);

        return factoryMock.Object;
    }

    private static bool IsPropertyExpression<TObject, TProperty>(Expression<Func<TObject, TProperty>> expression, string propertyName)
    {
        return expression.Body is MemberExpression { Member.Name: var name } && name == propertyName;
    }
}
