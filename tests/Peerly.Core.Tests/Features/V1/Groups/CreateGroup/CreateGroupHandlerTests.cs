using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Groups.CreateGroup;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Groups.CreateGroup;

public sealed class CreateGroupHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICommandValidator<CreateGroupCommand, CreateGroupCommandResponse>> _validatorMock = new();
    private readonly Mock<IClock> _clockMock = new();

    private readonly Fixture _fixture = new();
    private readonly CreateGroupHandler _handler;
    private readonly DateTimeOffset _currentTime;

    public CreateGroupHandlerTests()
    {
        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        _currentTime = _fixture.Create<DateTimeOffset>();
        _clockMock
            .Setup(clock => clock.GetCurrentTime())
            .Returns(_currentTime);

        _handler = new CreateGroupHandler(
            _unitOfWorkFactoryMock.Object,
            _validatorMock.Object,
            _clockMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultSuccess_ShouldCreateGroup()
    {
        // Arrange
        var command = _fixture.Create<CreateGroupCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok);

        var groupId = _fixture.Create<GroupId>();
        var expectedGroupAddItem = _fixture.Build<GroupAddItem>()
            .With(item => item.CourseId, command.CourseId)
            .With(item => item.Name, command.Name)
            .With(item => item.CreationTime, _currentTime)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.GroupRepository.AddAsync(expectedGroupAddItem, It.IsAny<CancellationToken>()))
            .ReturnsAsync(groupId);

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT0.Should().BeTrue();
        response.AsT0.GroupId.Should().Be(groupId);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.GroupRepository.AddAsync(expectedGroupAddItem, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultOtherError_ShouldBeOtherError()
    {
        // Arrange
        var command = _fixture.Create<CreateGroupCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtherError.PermissionDenied());

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT2.Should().BeTrue();
        response.AsT2.Type.Should().Be(ErrorType.PermissionDenied);
        response.AsT2.Message.Should().BeNull();
        _unitOfWorkFactoryMock.Verify(
            factory => factory.CreateAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.GroupRepository.AddAsync(It.IsAny<GroupAddItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultValidationError_ShouldBeValidationError()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();
        var command = _fixture.Create<CreateGroupCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationError.From(errorMessage));

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT1.Should().BeTrue();
        response.AsT1.Errors.Should().NotBeNull().And.ContainSingle(errorMessage);
        _unitOfWorkFactoryMock.Verify(
            factory => factory.CreateAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.GroupRepository.AddAsync(It.IsAny<GroupAddItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
