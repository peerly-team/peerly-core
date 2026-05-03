using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourseFile;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Files;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Courses.CreateCourseFile;

public sealed class CreateCourseFileHandlerTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IClock> _clockMock = new();
    private readonly Mock<ICommandValidator<CreateCourseFileCommand, CreateCourseFileCommandResponse>> _validatorMock = new();

    private readonly Fixture _fixture = new();
    private readonly CreateCourseFileHandler _handler;
    private readonly DateTimeOffset _currentTime;

    public CreateCourseFileHandlerTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();

        _currentTime = _fixture.Create<DateTimeOffset>();
        _handler = new CreateCourseFileHandler(
            unitOfWorkFactory,
            _validatorMock.Object,
            _clockMock.Object);
        _clockMock
            .Setup(clock => clock.GetCurrentTime())
            .Returns(_currentTime);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultSuccess_ShouldCreateCourseFile()
    {
        // Arrange
        var command = CreateCommand();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok);

        var fileId = _fixture.Create<FileId>();
        var expectedFileAddItem = _fixture.Build<FileAddItem>()
            .With(result => result.StorageId, command.StorageId)
            .With(result => result.Name, command.FileName)
            .With(result => result.Size, command.FileSize)
            .With(result => result.CreationTime, _currentTime)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.FileRepository.AddAsync(expectedFileAddItem, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileId);

        var expectedCourseFileAddItem = _fixture.Build<CourseFileAddItem>()
            .With(result => result.CourseId, command.CourseId)
            .With(result => result.FileId, fileId)
            .With(result => result.TeacherId, command.TeacherId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CourseFileRepository.AddAsync(expectedCourseFileAddItem, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fixture.Create<bool>());

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT0.Should().BeTrue();
        commandResponse.AsT0.FileId.Should().Be(fileId);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.FileRepository.AddAsync(expectedFileAddItem, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.CourseFileRepository.AddAsync(expectedCourseFileAddItem, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultOtherError_ShouldBeOtherError()
    {
        // Arrange
        var command = CreateCommand();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtherError.NotFound(CourseErrors.CourseNotFound));

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT2.Should().BeTrue();
        commandResponse.AsT2.Type.Should().Be(ErrorType.NotFound);
        commandResponse.AsT2.Message.Should().Be(CourseErrors.CourseNotFound);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.FileRepository.AddAsync(It.IsAny<FileAddItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.CourseFileRepository.AddAsync(It.IsAny<CourseFileAddItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var unitOfWorkFactoryMock = new Mock<ICommonUnitOfWorkFactory>();
        unitOfWorkFactoryMock
            .Setup(factory => factory.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        var operationSetMock = new Mock<IOperationSet>();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.StartOperationSet(It.IsAny<CancellationToken>()))
            .ReturnsAsync(operationSetMock.Object);

        return unitOfWorkFactoryMock.Object;
    }

    private CreateCourseFileCommand CreateCommand()
    {
        return _fixture.Build<CreateCourseFileCommand>()
            .With(command => command.StorageId, (StorageId)Guid.NewGuid())
            .Create();
    }
}
