using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OneOf.Types;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.DeleteHomeworkFile;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Homeworks.DeleteHomeworkFile;

public sealed class DeleteHomeworkFileHandlerTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IHomeworkFileRepository> _homeworkFileRepositoryMock = new();
    private readonly Mock<ICommandValidator<DeleteHomeworkFileCommand, Success>> _validatorMock = new();

    private readonly Fixture _fixture = new();
    private readonly DeleteHomeworkFileHandler _handler;

    public DeleteHomeworkFileHandlerTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _handler = new DeleteHomeworkFileHandler(unitOfWorkFactory, _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultSuccess_ShouldDeleteHomeworkFile()
    {
        // Arrange
        var command = _fixture.Create<DeleteHomeworkFileCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok);

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT0.Should().BeTrue();
        _homeworkFileRepositoryMock.Verify(
            repository => repository.DeleteAsync(command.HomeworkId, command.FileId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultOtherError_ShouldBeOtherError()
    {
        // Arrange
        var command = _fixture.Create<DeleteHomeworkFileCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtherError.NotFound(HomeworkErrors.HomeworkNotFound));

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT2.Should().BeTrue();
        commandResponse.AsT2.Type.Should().Be(ErrorType.NotFound);
        commandResponse.AsT2.Message.Should().Be(HomeworkErrors.HomeworkNotFound);
        _homeworkFileRepositoryMock.Verify(
            repository => repository.DeleteAsync(It.IsAny<HomeworkId>(), It.IsAny<FileId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultValidationError_ShouldBeValidationError()
    {
        // Arrange
        var command = _fixture.Create<DeleteHomeworkFileCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationError.From(HomeworkErrors.IncorrectHomeworkStatusForDeleteFile));

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT1.Should().BeTrue();
        commandResponse.AsT1.Errors.Should().NotBeNull().And.ContainSingle(HomeworkErrors.IncorrectHomeworkStatusForDeleteFile.Value);
        _homeworkFileRepositoryMock.Verify(
            repository => repository.DeleteAsync(It.IsAny<HomeworkId>(), It.IsAny<FileId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var unitOfWorkFactoryMock = new Mock<ICommonUnitOfWorkFactory>();
        unitOfWorkFactoryMock
            .Setup(factory => factory.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.HomeworkFileRepository)
            .Returns(_homeworkFileRepositoryMock.Object);

        return unitOfWorkFactoryMock.Object;
    }
}
