using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OneOf.Types;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.DeleteHomework;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Homeworks.DeleteHomework;

public sealed class DeleteHomeworkHandlerTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IHomeworkFileRepository> _homeworkFileRepositoryMock = new();
    private readonly Mock<IHomeworkRepository> _homeworkRepositoryMock = new();
    private readonly Mock<ICommandValidator<DeleteHomeworkCommand, Success>> _validatorMock = new();

    private readonly Fixture _fixture = new();
    private readonly DeleteHomeworkHandler _handler;

    public DeleteHomeworkHandlerTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _handler = new DeleteHomeworkHandler(unitOfWorkFactory, _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultSuccess_ShouldDeleteHomeworkFilesAndHomework()
    {
        // Arrange
        var command = _fixture.Create<DeleteHomeworkCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok);

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT0.Should().BeTrue();
        _homeworkFileRepositoryMock.Verify(
            repository => repository.DeleteByHomeworkAsync(
                command.HomeworkId,
                It.IsAny<CancellationToken>()),
            Times.Once);
        _homeworkRepositoryMock.Verify(
            repository => repository.DeleteAsync(command.HomeworkId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultOtherError_ShouldBeOtherError()
    {
        // Arrange
        var command = _fixture.Create<DeleteHomeworkCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtherError.NotFound(HomeworkErrors.HomeworkNotFound));

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT2.Should().BeTrue();
        commandResponse.AsT2.Type.Should().Be(ErrorType.NotFound);
        commandResponse.AsT2.Message.Should().Be(HomeworkErrors.HomeworkNotFound);
        VerifyDeleteNeverCalled();
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultValidationError_ShouldBeValidationError()
    {
        // Arrange
        var command = _fixture.Create<DeleteHomeworkCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationError.From(HomeworkErrors.IncorrectHomeworkStatusForDelete));

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT1.Should().BeTrue();
        commandResponse.AsT1.Errors.Should().NotBeNull().And.ContainSingle(HomeworkErrors.IncorrectHomeworkStatusForDelete.Value);
        VerifyDeleteNeverCalled();
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
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.HomeworkFileRepository)
            .Returns(_homeworkFileRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.HomeworkRepository)
            .Returns(_homeworkRepositoryMock.Object);

        return unitOfWorkFactoryMock.Object;
    }

    private void VerifyDeleteNeverCalled()
    {
        _homeworkFileRepositoryMock.Verify(
            repository => repository.DeleteByHomeworkAsync(
                It.IsAny<HomeworkId>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _homeworkRepositoryMock.Verify(
            repository => repository.DeleteAsync(It.IsAny<HomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
