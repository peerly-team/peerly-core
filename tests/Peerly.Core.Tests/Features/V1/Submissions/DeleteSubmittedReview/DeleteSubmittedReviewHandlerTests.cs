using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OneOf.Types;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedReview;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.DeleteSubmittedReview;

public sealed class DeleteSubmittedReviewHandlerTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ISubmittedReviewRepository> _submittedReviewRepositoryMock = new();
    private readonly Mock<ISubmittedReviewScoreRepository> _submittedReviewScoreRepositoryMock = new();
    private readonly Mock<IOperationSet> _operationSetMock = new();
    private readonly Mock<ICommandValidator<DeleteSubmittedReviewCommand, Success>> _validatorMock = new();
    private readonly Fixture _fixture = new();
    private readonly DeleteSubmittedReviewHandler _handler;

    public DeleteSubmittedReviewHandlerTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _handler = new DeleteSubmittedReviewHandler(unitOfWorkFactory, _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationSuccess_ShouldDeleteSubmittedReview()
    {
        // Arrange
        var command = _fixture.Create<DeleteSubmittedReviewCommand>();
        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok());

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT0.Should().BeTrue();
        _submittedReviewRepositoryMock.Verify(
            repository => repository.DeleteAsync(command.SubmittedReviewId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultOtherError_ShouldBeOtherError()
    {
        // Arrange
        var command = _fixture.Create<DeleteSubmittedReviewCommand>();
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
            repository => repository.DeleteAsync(It.IsAny<SubmittedReviewId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultValidationError_ShouldBeValidationError()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();
        var command = _fixture.Create<DeleteSubmittedReviewCommand>();
        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationError.From(errorMessage));

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT1.Should().BeTrue();
        response.AsT1.Errors.Should().NotBeNull().And.ContainSingle(errorMessage);
        _submittedReviewRepositoryMock.Verify(
            repository => repository.DeleteAsync(It.IsAny<SubmittedReviewId>(), It.IsAny<CancellationToken>()),
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
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.SubmittedReviewScoreRepository)
            .Returns(_submittedReviewScoreRepositoryMock.Object);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.StartOperationSet(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_operationSetMock.Object);

        return factoryMock.Object;
    }
}
