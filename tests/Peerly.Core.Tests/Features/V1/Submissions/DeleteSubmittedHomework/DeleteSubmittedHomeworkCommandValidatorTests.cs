using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Submissions;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.DeleteSubmittedHomework;

public sealed class DeleteSubmittedHomeworkCommandValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Fixture _fixture = new();
    private readonly DeleteSubmittedHomeworkValidator _validator;

    public DeleteSubmittedHomeworkCommandValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new DeleteSubmittedHomeworkValidator(unitOfWorkFactory);
    }

    [Fact]
    public async Task ValidateAsync_SubmittedHomeworkBelongsToStudentAndHomeworkPublished_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Create<DeleteSubmittedHomeworkCommand>();

        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(result => result.Id, command.SubmittedHomeworkId)
            .With(result => result.StudentId, command.StudentId)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomework);

        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, submittedHomework.HomeworkId)
            .With(result => result.Status, HomeworkStatus.Published)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_SubmittedHomeworkNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<DeleteSubmittedHomeworkCommand>();

        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubmittedHomework?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(SubmittedHomeworkErrors.SubmittedHomeworkNotFound);
        _unitOfWorkMock.Verify(
            uow => uow.ReadOnlyHomeworkRepository.GetAsync(It.IsAny<HomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_SubmittedHomeworkBelongsToAnotherStudent_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<DeleteSubmittedHomeworkCommand>();

        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(result => result.Id, command.SubmittedHomeworkId)
            .With(result => result.StudentId, (StudentId)((long)command.StudentId + 1))
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomework);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(SubmittedHomeworkErrors.SubmittedHomeworkNotFound);
        _unitOfWorkMock.Verify(
            uow => uow.ReadOnlyHomeworkRepository.GetAsync(It.IsAny<HomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_HomeworkNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<DeleteSubmittedHomeworkCommand>();

        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(result => result.Id, command.SubmittedHomeworkId)
            .With(result => result.StudentId, command.StudentId)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomework);
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Homework?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(HomeworkErrors.HomeworkNotFound);
    }

    [Theory]
    [InlineData(HomeworkStatus.Draft)]
    [InlineData(HomeworkStatus.Reviewing)]
    [InlineData(HomeworkStatus.Confirmation)]
    [InlineData(HomeworkStatus.Finished)]
    public async Task ValidateAsync_HomeworkNotInPublishedStatus_ShouldBeOtherErrorConflict(HomeworkStatus status)
    {
        // Arrange
        var command = _fixture.Create<DeleteSubmittedHomeworkCommand>();

        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(result => result.Id, command.SubmittedHomeworkId)
            .With(result => result.StudentId, command.StudentId)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomework);

        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, submittedHomework.HomeworkId)
            .With(result => result.Status, status)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Errors.Should().NotBeNull().And.ContainSingle(HomeworkErrors.IncorrectHomeworkStatusForDelete);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var factoryMock = new Mock<ICommonUnitOfWorkFactory>();
        factoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        return factoryMock.Object;
    }
}
