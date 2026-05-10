using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.DeleteHomeworkFile;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Homeworks.DeleteHomeworkFile;

public sealed class DeleteHomeworkFileCommandValidatorTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Fixture _fixture = new();
    private readonly DeleteHomeworkFileCommandValidator _validator;

    public DeleteHomeworkFileCommandValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new DeleteHomeworkFileCommandValidator(unitOfWorkFactory);
    }

    [Fact]
    public async Task ValidateAsync_HomeworkTeacherMatchesAndHomeworkInDraftStatus_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Create<DeleteHomeworkFileCommand>();

        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, command.HomeworkId)
            .With(result => result.TeacherId, command.TeacherId)
            .With(result => result.Status, HomeworkStatus.Draft)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.HomeworkRepository.GetAsync(command.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_HomeworkNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<DeleteHomeworkFileCommand>();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.HomeworkRepository.GetAsync(command.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Homework?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(HomeworkErrors.HomeworkNotFound);
    }

    [Fact]
    public async Task ValidateAsync_HomeworkTeacherDoesNotMatch_ShouldBeOtherErrorPermissionDenied()
    {
        // Arrange
        var command = _fixture.Create<DeleteHomeworkFileCommand>();

        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, command.HomeworkId)
            .With(result => result.TeacherId, (TeacherId)((long)command.TeacherId + 1))
            .With(result => result.Status, HomeworkStatus.Draft)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.HomeworkRepository.GetAsync(command.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.PermissionDenied);
        result.AsT2.Message.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_HomeworkNotInDraftStatus_ShouldBeValidationError()
    {
        // Arrange
        var command = _fixture.Create<DeleteHomeworkFileCommand>();

        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, command.HomeworkId)
            .With(result => result.TeacherId, command.TeacherId)
            .With(result => result.Status, HomeworkStatus.Published)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.HomeworkRepository.GetAsync(command.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Errors.Should().NotBeNull().And.ContainSingle(HomeworkErrors.IncorrectHomeworkStatusForDelete);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var unitOfWorkFactoryMock = new Mock<ICommonUnitOfWorkFactory>();
        unitOfWorkFactoryMock
            .Setup(factory => factory.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        return unitOfWorkFactoryMock.Object;
    }
}
