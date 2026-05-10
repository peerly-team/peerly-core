using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OneOf.Types;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomeworkFile;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.DeleteSubmittedHomeworkFile;

public sealed class DeleteSubmittedHomeworkFileHandlerTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ISubmittedHomeworkFileRepository> _submittedHomeworkFileRepositoryMock = new();
    private readonly Mock<ICommandValidator<DeleteSubmittedHomeworkFileCommand, Success>> _validatorMock = new();
    private readonly Fixture _fixture = new();
    private readonly DeleteSubmittedHomeworkFileHandler _handler;

    public DeleteSubmittedHomeworkFileHandlerTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _handler = new DeleteSubmittedHomeworkFileHandler(unitOfWorkFactory, _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationSuccess_ShouldDeleteSubmittedHomeworkFile()
    {
        // Arrange
        var command = _fixture.Create<DeleteSubmittedHomeworkFileCommand>();
        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok());

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT0.Should().BeTrue();
        _submittedHomeworkFileRepositoryMock.Verify(
            repository => repository.DeleteAsync(command.SubmittedHomeworkId, command.FileId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultOtherError_ShouldBeOtherError()
    {
        // Arrange
        var command = _fixture.Create<DeleteSubmittedHomeworkFileCommand>();
        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtherError.NotFound(SubmittedHomeworkErrors.SubmittedHomeworkNotFound));

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT2.Should().BeTrue();
        response.AsT2.Type.Should().Be(ErrorType.NotFound);
        response.AsT2.Message.Should().Be(SubmittedHomeworkErrors.SubmittedHomeworkNotFound);
        _submittedHomeworkFileRepositoryMock.Verify(
            repository => repository.DeleteAsync(It.IsAny<SubmittedHomeworkId>(), It.IsAny<FileId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultValidationError_ShouldBeValidationError()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();
        var command = _fixture.Create<DeleteSubmittedHomeworkFileCommand>();
        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationError.From(errorMessage));

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT1.Should().BeTrue();
        response.AsT1.Errors.Should().NotBeNull().And.ContainSingle(errorMessage);
        _submittedHomeworkFileRepositoryMock.Verify(
            repository => repository.DeleteAsync(It.IsAny<SubmittedHomeworkId>(), It.IsAny<FileId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var factoryMock = new Mock<ICommonUnitOfWorkFactory>();
        factoryMock
            .Setup(factory => factory.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.SubmittedHomeworkFileRepository)
            .Returns(_submittedHomeworkFileRepositoryMock.Object);

        return factoryMock.Object;
    }
}
