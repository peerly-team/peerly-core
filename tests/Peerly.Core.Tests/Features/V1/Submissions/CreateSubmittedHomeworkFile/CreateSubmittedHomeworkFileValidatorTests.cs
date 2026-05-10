using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Submissions;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.CreateSubmittedHomeworkFile;

public sealed class CreateSubmittedHomeworkFileValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IClock> _clockMock = new();
    private readonly Fixture _fixture = new();
    private readonly CreateSubmittedHomeworkFileValidator _validator;

    public CreateSubmittedHomeworkFileValidatorTests()
    {
        _fixture.Customize(new SupportMutableValueTypesCustomization());

        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new CreateSubmittedHomeworkFileValidator(unitOfWorkFactory, _clockMock.Object);

        _clockMock
            .Setup(c => c.GetCurrentTime())
            .Returns(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task ValidateAsync_CourseHomeworkIsValid_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkFileCommand>();

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
            .With(result => result.Deadline, DateTimeOffset.UtcNow.AddDays(1))
            .Without(result => result.GroupId)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        var courseStudent = new CourseStudent { CourseId = homework.CourseId, StudentId = command.StudentId };
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyGroupStudentRepository.ExistsAsync(courseStudent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_GroupHomeworkIsValid_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkFileCommand>();
        var groupId = _fixture.Create<GroupId>();

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
            .With(result => result.GroupId, groupId)
            .With(result => result.Deadline, DateTimeOffset.UtcNow.AddDays(1))
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        var groupStudent = new GroupStudent { GroupId = groupId, StudentId = command.StudentId };
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyGroupStudentRepository.ExistsAsync(groupStudent, It.IsAny<CancellationToken>()))
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
        var command = _fixture.Create<CreateSubmittedHomeworkFileCommand>();

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
    public async Task ValidateAsync_WrongStudent_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkFileCommand>();
        var otherStudentId = _fixture.Create<StudentId>();

        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(result => result.Id, command.SubmittedHomeworkId)
            .With(result => result.StudentId, otherStudentId)
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
        var command = _fixture.Create<CreateSubmittedHomeworkFileCommand>();

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

        _unitOfWorkMock.Verify(
            uow => uow.ReadOnlyGroupStudentRepository.ExistsAsync(It.IsAny<GroupStudent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_HomeworkIsDraft_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkFileCommand>();

        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(result => result.Id, command.SubmittedHomeworkId)
            .With(result => result.StudentId, command.StudentId)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomework);

        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, submittedHomework.HomeworkId)
            .With(result => result.Status, HomeworkStatus.Draft)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(HomeworkErrors.HomeworkNotFound);

        _unitOfWorkMock.Verify(
            uow => uow.ReadOnlyGroupStudentRepository.ExistsAsync(It.IsAny<GroupStudent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_CourseStudentNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkFileCommand>();

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
            .With(result => result.GroupId, (GroupId?)null)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
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
        var command = _fixture.Create<CreateSubmittedHomeworkFileCommand>();
        var groupId = _fixture.Create<GroupId>();

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
            .With(result => result.GroupId, groupId)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
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

    [Theory]
    [InlineData(HomeworkStatus.Reviewing)]
    [InlineData(HomeworkStatus.Confirmation)]
    [InlineData(HomeworkStatus.Finished)]
    public async Task ValidateAsync_HomeworkNotPublished_ShouldBeOtherErrorConflict(HomeworkStatus homeworkStatus)
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkFileCommand>();

        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(result => result.Id, command.SubmittedHomeworkId)
            .With(result => result.StudentId, command.StudentId)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomework);

        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, submittedHomework.HomeworkId)
            .With(result => result.Status, homeworkStatus)
            .With(result => result.GroupId, (GroupId?)null)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
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
        var command = _fixture.Create<CreateSubmittedHomeworkFileCommand>();

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
            .With(result => result.GroupId, (GroupId?)null)
            .With(result => result.Deadline, DateTimeOffset.UtcNow.AddDays(-1))
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
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

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var unitOfWorkFactoryMock = new Mock<ICommonUnitOfWorkFactory>();
        unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);
        return unitOfWorkFactoryMock.Object;
    }
}
