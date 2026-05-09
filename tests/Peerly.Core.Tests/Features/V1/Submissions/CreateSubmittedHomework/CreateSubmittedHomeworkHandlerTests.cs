using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomework;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.CreateSubmittedHomework;

public sealed class CreateSubmittedHomeworkHandlerTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IClock> _clockMock = new();
    private readonly Mock<ICommandValidator<CreateSubmittedHomeworkCommand, CreateSubmittedHomeworkCommandResponse>> _validatorMock = new();
    private readonly Fixture _fixture = new();
    private readonly CreateSubmittedHomeworkHandler _handler;
    private readonly DateTimeOffset _currentTime;

    public CreateSubmittedHomeworkHandlerTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _currentTime = _fixture.Create<DateTimeOffset>();
        _clockMock.Setup(clock => clock.GetCurrentTime()).Returns(_currentTime);
        _handler = new CreateSubmittedHomeworkHandler(unitOfWorkFactory, _clockMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationSuccess_ShouldAddSubmittedHomeworkAndReturnId()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkCommand>();
        var expectedId = _fixture.Create<SubmittedHomeworkId>();

        _validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok);

        _unitOfWorkMock
            .Setup(uow => uow.SubmittedHomeworkRepository.AddAsync(
                It.Is<SubmittedHomeworkAddItem>(item =>
                    item.HomeworkId == command.HomeworkId &&
                    item.StudentId == command.StudentId &&
                    item.Comment == command.Comment &&
                    item.CreationTime == _currentTime),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT0.Should().BeTrue();
        response.AsT0.SubmittedHomeworkId.Should().Be(expectedId);

        _unitOfWorkMock.Verify(
            uow => uow.SubmittedHomeworkRepository.AddAsync(It.IsAny<SubmittedHomeworkAddItem>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultOtherError_ShouldBeOtherError()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkCommand>();

        _validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtherError.PermissionDenied());

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT2.Should().BeTrue();
        response.AsT2.Type.Should().Be(ErrorType.PermissionDenied);
        response.AsT2.Message.Should().BeNull();
        _unitOfWorkMock.Verify(
            uow => uow.SubmittedHomeworkRepository.AddAsync(It.IsAny<SubmittedHomeworkAddItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultValidationError_ShouldBeValidationError()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();
        var command = _fixture.Create<CreateSubmittedHomeworkCommand>();

        _validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationError.From(errorMessage));

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT1.Should().BeTrue();
        response.AsT1.Errors.Should().NotBeNull().And.ContainSingle(errorMessage);
        _unitOfWorkMock.Verify(
            uow => uow.SubmittedHomeworkRepository.AddAsync(It.IsAny<SubmittedHomeworkAddItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var factoryMock = new Mock<ICommonUnitOfWorkFactory>();
        factoryMock
            .Setup(f => f.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);
        return factoryMock.Object;
    }
}
