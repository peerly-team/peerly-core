using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.CreateSubmittedHomework;

public sealed class CreateSubmittedHomeworkCommandValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IClock> _clockMock = new();
    private readonly Fixture _fixture = new();
    private readonly CreateSubmittedHomeworkCommandValidator _validator;

    public CreateSubmittedHomeworkCommandValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new CreateSubmittedHomeworkCommandValidator(unitOfWorkFactory, _clockMock.Object);

        _clockMock.Setup(c => c.GetCurrentTime()).Returns(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task ValidateAsync_CourseHomeworkIsValid_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkCommand>();

        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, command.HomeworkId)
            .With(result => result.Status, HomeworkStatus.Published)
            .With(result => result.GroupId, (GroupId?)null)
            .With(result => result.Deadline, DateTimeOffset.UtcNow.AddDays(1))
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(homework.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        var courseStudent = new CourseStudent { CourseId = homework.CourseId, StudentId = command.StudentId };
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyGroupStudentRepository.ExistsAsync(courseStudent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var homeworkStudent = new HomeworkStudent { HomeworkId = command.HomeworkId, StudentId = command.StudentId };
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlySubmittedHomeworkRepository.GetSubmittedHomeworkIdAsync(homeworkStudent, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubmittedHomeworkId?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_GroupHomeworkIsValid_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkCommand>();

        var groupId = _fixture.Create<GroupId>();
        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, command.HomeworkId)
            .With(result => result.Status, HomeworkStatus.Published)
            .With(result => result.GroupId, groupId)
            .With(result => result.Deadline, DateTimeOffset.UtcNow.AddDays(1))
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(homework.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        var groupStudent = new GroupStudent { GroupId = groupId, StudentId = command.StudentId };
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyGroupStudentRepository.ExistsAsync(groupStudent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var homeworkStudent = new HomeworkStudent { HomeworkId = command.HomeworkId, StudentId = command.StudentId };
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlySubmittedHomeworkRepository.GetSubmittedHomeworkIdAsync(homeworkStudent, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubmittedHomeworkId?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_HomeworkNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkCommand>();

        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(command.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Homework?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(HomeworkErrors.HomeworkNotFound);

        _unitOfWorkMock.Verify(
            uow => uow.ReadOnlyGroupStudentRepository.ExistsAsync(It.IsAny<CourseStudent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_CourseStudentNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkCommand>();

        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, command.HomeworkId)
            .With(result => result.Status, HomeworkStatus.Published)
            .With(result => result.GroupId, (GroupId?)null)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(homework.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        var courseStudent = new CourseStudent { CourseId = homework.CourseId, StudentId = command.StudentId };
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyGroupStudentRepository.ExistsAsync(courseStudent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(HomeworkErrors.HomeworkNotFound);
    }

    [Fact]
    public async Task ValidateAsync_GroupStudentNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkCommand>();
        var groupId = _fixture.Create<GroupId>();

        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, command.HomeworkId)
            .With(result => result.Status, HomeworkStatus.Published)
            .With(result => result.GroupId, groupId)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(homework.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        var groupStudent = new GroupStudent { GroupId = groupId, StudentId = command.StudentId };
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyGroupStudentRepository.ExistsAsync(groupStudent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(HomeworkErrors.HomeworkNotFound);
    }

    [Fact]
    public async Task ValidateAsync_HomeworkIsDraft_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkCommand>();

        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, command.HomeworkId)
            .With(result => result.Status, HomeworkStatus.Draft)
            .With(result => result.GroupId, (GroupId?)null)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(homework.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(HomeworkErrors.HomeworkNotFound);

        _unitOfWorkMock.Verify(
            uow => uow.ReadOnlyGroupStudentRepository.ExistsAsync(It.IsAny<CourseStudent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_HomeworkIsNotPublished_ShouldBeOtherErrorConflict()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkCommand>();

        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, command.HomeworkId)
            .With(result => result.Status, HomeworkStatus.Reviewing)
            .With(result => result.GroupId, (GroupId?)null)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(homework.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        var courseStudent = new CourseStudent { CourseId = homework.CourseId, StudentId = command.StudentId };
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyGroupStudentRepository.ExistsAsync(courseStudent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.Conflict);
        result.AsT2.Message.Should().Be(HomeworkErrors.HomeworkNotAcceptingSubmissions);
    }

    [Fact]
    public async Task ValidateAsync_DeadlinePassed_ShouldBeOtherErrorConflict()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkCommand>();

        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, command.HomeworkId)
            .With(result => result.Status, HomeworkStatus.Published)
            .With(result => result.GroupId, (GroupId?)null)
            .With(result => result.Deadline, DateTimeOffset.UtcNow.AddDays(-1))
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(homework.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        var courseStudent = new CourseStudent { CourseId = homework.CourseId, StudentId = command.StudentId };
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyGroupStudentRepository.ExistsAsync(courseStudent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.Conflict);
        result.AsT2.Message.Should().Be(HomeworkErrors.HomeworkNotAcceptingSubmissions);
    }

    [Fact]
    public async Task ValidateAsync_SubmittedHomeworkAlreadyExists_ShouldBeOtherErrorConflict()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkCommand>();

        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, command.HomeworkId)
            .With(result => result.Status, HomeworkStatus.Published)
            .With(result => result.GroupId, (GroupId?)null)
            .With(result => result.Deadline, DateTimeOffset.UtcNow.AddDays(1))
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(homework.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        var courseStudent = new CourseStudent { CourseId = homework.CourseId, StudentId = command.StudentId };
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyGroupStudentRepository.ExistsAsync(courseStudent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var homeworkStudent = new HomeworkStudent { HomeworkId = command.HomeworkId, StudentId = command.StudentId };
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlySubmittedHomeworkRepository.GetSubmittedHomeworkIdAsync(homeworkStudent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fixture.Create<SubmittedHomeworkId>());

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.Conflict);
        result.AsT2.Message.Should().Be(SubmittedHomeworkErrors.SubmittedHomeworkAlreadySubmitted);
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
