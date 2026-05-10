using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.CreateSubmittedReview;

public sealed class CreateSubmittedReviewHandlerTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ISubmittedReviewRepository> _submittedReviewRepositoryMock = new();
    private readonly Mock<IClock> _clockMock = new();
    private readonly Mock<ICommandValidator<CreateSubmittedReviewCommand, CreateSubmittedReviewCommandResponse>> _validatorMock = new();
    private readonly Fixture _fixture = new();
    private readonly CreateSubmittedReviewHandler _handler;
    private readonly DateTimeOffset _currentTime;

    public CreateSubmittedReviewHandlerTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _currentTime = _fixture.Create<DateTimeOffset>();
        _clockMock.Setup(clock => clock.GetCurrentTime()).Returns(_currentTime);
        _handler = new CreateSubmittedReviewHandler(unitOfWorkFactory, _clockMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationSuccess_ShouldAddSubmittedReviewAndReturnId()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedReviewCommand>();
        var expectedId = _fixture.Create<SubmittedReviewId>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok);

        _submittedReviewRepositoryMock
            .Setup(repository => repository.AddAsync(
                It.Is<SubmittedReviewAddItem>(item =>
                    item.SubmittedHomeworkId == command.SubmittedHomeworkId &&
                    item.StudentId == command.StudentId &&
                    item.Mark == command.Mark &&
                    item.Comment == command.Comment &&
                    item.CreationTime == _currentTime),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT0.Should().BeTrue();
        response.AsT0.SubmittedReviewId.Should().Be(expectedId);
        _submittedReviewRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<SubmittedReviewAddItem>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultOtherError_ShouldBeOtherError()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedReviewCommand>();
        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtherError.PermissionDenied());

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT2.Should().BeTrue();
        response.AsT2.Type.Should().Be(ErrorType.PermissionDenied);
        _submittedReviewRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<SubmittedReviewAddItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultValidationError_ShouldBeValidationError()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();
        var command = _fixture.Create<CreateSubmittedReviewCommand>();
        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationError.From(errorMessage));

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT1.Should().BeTrue();
        response.AsT1.Errors.Should().NotBeNull().And.ContainSingle(errorMessage);
        _submittedReviewRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<SubmittedReviewAddItem>(), It.IsAny<CancellationToken>()),
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
}
