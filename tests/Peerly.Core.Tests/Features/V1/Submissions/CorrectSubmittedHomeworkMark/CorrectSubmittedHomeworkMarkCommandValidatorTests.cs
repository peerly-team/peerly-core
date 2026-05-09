using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CorrectSubmittedHomeworkMark;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Submissions;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.CorrectSubmittedHomeworkMark;

public sealed class CorrectSubmittedHomeworkMarkCommandValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Fixture _fixture = new();
    private readonly CorrectSubmittedHomeworkMarkCommandValidator _validator;

    public CorrectSubmittedHomeworkMarkCommandValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new CorrectSubmittedHomeworkMarkCommandValidator(unitOfWorkFactory);
    }

    [Fact]
    public async Task ValidateAsync_TeacherIsCourseTeacherAndHomeworkInConfirmation_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Create<CorrectSubmittedHomeworkMarkCommand>();

        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(x => x.Id, command.SubmittedHomeworkId)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomework);

        var homework = _fixture.Build<Homework>()
            .With(x => x.Id, submittedHomework.HomeworkId)
            .With(x => x.Status, HomeworkStatus.Confirmation)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        var courseTeacher = new CourseTeacher { CourseId = homework.CourseId, TeacherId = command.TeacherId };
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_TeacherIsGroupTeacherAndHomeworkInConfirmation_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Create<CorrectSubmittedHomeworkMarkCommand>();

        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(x => x.Id, command.SubmittedHomeworkId)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomework);

        var homework = _fixture.Build<Homework>()
            .With(x => x.Id, submittedHomework.HomeworkId)
            .With(x => x.Status, HomeworkStatus.Confirmation)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        var courseTeacher = new CourseTeacher { CourseId = homework.CourseId, TeacherId = command.TeacherId };
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyGroupTeacherRepository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_SubmittedHomeworkNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<CorrectSubmittedHomeworkMarkCommand>();

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
    public async Task ValidateAsync_HomeworkNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<CorrectSubmittedHomeworkMarkCommand>();

        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(x => x.Id, command.SubmittedHomeworkId)
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

        _unitOfWorkMock.Verify(
            uow => uow.ReadOnlyCourseTeacherRepository.ExistsAsync(It.IsAny<CourseTeacher>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_TeacherIsNotCourseOrGroupTeacher_ShouldBeOtherErrorPermissionDenied()
    {
        // Arrange
        var command = _fixture.Create<CorrectSubmittedHomeworkMarkCommand>();

        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(x => x.Id, command.SubmittedHomeworkId)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomework);

        var homework = _fixture.Build<Homework>()
            .With(x => x.Id, submittedHomework.HomeworkId)
            .With(x => x.Status, HomeworkStatus.Confirmation)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        var courseTeacher = new CourseTeacher { CourseId = homework.CourseId, TeacherId = command.TeacherId };
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyGroupTeacherRepository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.PermissionDenied);
        result.AsT2.Message.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_HomeworkNotInConfirmationStatus_ShouldBeOtherErrorConflict()
    {
        // Arrange
        var command = _fixture.Create<CorrectSubmittedHomeworkMarkCommand>();

        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(x => x.Id, command.SubmittedHomeworkId)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomework);

        var homework = _fixture.Build<Homework>()
            .With(x => x.Id, submittedHomework.HomeworkId)
            .With(x => x.Status, HomeworkStatus.Reviewing)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        var courseTeacher = new CourseTeacher { CourseId = homework.CourseId, TeacherId = command.TeacherId };
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.Conflict);
        result.AsT2.Message.Should().Be(HomeworkErrors.HomeworkNotInConfirmationStatus);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var unitOfWorkFactoryMock = new Mock<ICommonUnitOfWorkFactory>();
        unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);
        return unitOfWorkFactoryMock.Object;
    }
}
