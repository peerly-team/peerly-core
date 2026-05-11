using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.UpdateSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Submissions;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.UpdateSubmittedHomework;

public sealed class UpdateSubmittedHomeworkCommandValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IClock> _clockMock = new();
    private readonly Fixture _fixture = new();
    private readonly UpdateSubmittedHomeworkCommandValidator _validator;

    public UpdateSubmittedHomeworkCommandValidatorTests()
    {
        _fixture.Customize(new SupportMutableValueTypesCustomization());

        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new UpdateSubmittedHomeworkCommandValidator(unitOfWorkFactory, _clockMock.Object);

        _clockMock
            .Setup(clock => clock.GetCurrentTime())
            .Returns(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task ValidateAsync_CourseHomeworkIsValid_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Create<UpdateSubmittedHomeworkCommand>();
        var submittedHomework = SetupSubmittedHomework(command);
        var homework = SetupHomework(submittedHomework, HomeworkStatus.Published, groupId: null, deadline: DateTimeOffset.UtcNow.AddDays(1));

        var courseStudent = new CourseStudent { CourseId = homework.CourseId, StudentId = command.StudentId };
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupStudentRepository.ExistsAsync(courseStudent, It.IsAny<CancellationToken>()))
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
        var command = _fixture.Create<UpdateSubmittedHomeworkCommand>();
        var groupId = _fixture.Create<GroupId>();
        var submittedHomework = SetupSubmittedHomework(command);
        SetupHomework(submittedHomework, HomeworkStatus.Published, groupId, DateTimeOffset.UtcNow.AddDays(1));

        var groupStudent = new GroupStudent { GroupId = groupId, StudentId = command.StudentId };
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupStudentRepository.ExistsAsync(groupStudent, It.IsAny<CancellationToken>()))
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
        var command = _fixture.Create<UpdateSubmittedHomeworkCommand>();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubmittedHomework?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(SubmittedHomeworkErrors.SubmittedHomeworkNotFound);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyHomeworkRepository.GetAsync(It.IsAny<HomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_WrongStudent_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<UpdateSubmittedHomeworkCommand>();
        var otherStudentId = _fixture.Create<StudentId>();

        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(homework => homework.Id, command.SubmittedHomeworkId)
            .With(homework => homework.StudentId, otherStudentId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomework);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(SubmittedHomeworkErrors.SubmittedHomeworkNotFound);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyHomeworkRepository.GetAsync(It.IsAny<HomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_HomeworkNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<UpdateSubmittedHomeworkCommand>();
        var submittedHomework = SetupSubmittedHomework(command);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Homework?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(HomeworkErrors.HomeworkNotFound);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyGroupStudentRepository.ExistsAsync(It.IsAny<GroupStudent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_HomeworkIsDraft_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<UpdateSubmittedHomeworkCommand>();
        var submittedHomework = SetupSubmittedHomework(command);
        SetupHomework(submittedHomework, HomeworkStatus.Draft);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(HomeworkErrors.HomeworkNotFound);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyGroupStudentRepository.ExistsAsync(It.IsAny<GroupStudent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_CourseStudentNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<UpdateSubmittedHomeworkCommand>();
        var submittedHomework = SetupSubmittedHomework(command);
        var homework = SetupHomework(submittedHomework, HomeworkStatus.Published, groupId: null);

        var courseStudent = new CourseStudent { CourseId = homework.CourseId, StudentId = command.StudentId };
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupStudentRepository.ExistsAsync(courseStudent, It.IsAny<CancellationToken>()))
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
        var command = _fixture.Create<UpdateSubmittedHomeworkCommand>();
        var groupId = _fixture.Create<GroupId>();
        var submittedHomework = SetupSubmittedHomework(command);
        SetupHomework(submittedHomework, HomeworkStatus.Published, groupId);

        var groupStudent = new GroupStudent { GroupId = groupId, StudentId = command.StudentId };
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupStudentRepository.ExistsAsync(groupStudent, It.IsAny<CancellationToken>()))
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
        var command = _fixture.Create<UpdateSubmittedHomeworkCommand>();
        var submittedHomework = SetupSubmittedHomework(command);
        var homework = SetupHomework(submittedHomework, homeworkStatus, groupId: null);

        var courseStudent = new CourseStudent { CourseId = homework.CourseId, StudentId = command.StudentId };
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupStudentRepository.ExistsAsync(courseStudent, It.IsAny<CancellationToken>()))
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
        var command = _fixture.Create<UpdateSubmittedHomeworkCommand>();
        var submittedHomework = SetupSubmittedHomework(command);
        var homework = SetupHomework(submittedHomework, HomeworkStatus.Published, groupId: null, deadline: DateTimeOffset.UtcNow.AddDays(-1));

        var courseStudent = new CourseStudent { CourseId = homework.CourseId, StudentId = command.StudentId };
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupStudentRepository.ExistsAsync(courseStudent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.Conflict);
        result.AsT2.Message.Should().Be(HomeworkErrors.HomeworkNotAcceptingSubmissions);
    }

    private SubmittedHomework SetupSubmittedHomework(UpdateSubmittedHomeworkCommand command)
    {
        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(homework => homework.Id, command.SubmittedHomeworkId)
            .With(homework => homework.StudentId, command.StudentId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomework);

        return submittedHomework;
    }

    private Homework SetupHomework(
        SubmittedHomework submittedHomework,
        HomeworkStatus status,
        GroupId? groupId = null,
        DateTimeOffset? deadline = null)
    {
        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, submittedHomework.HomeworkId)
            .With(result => result.Status, status)
            .With(result => result.GroupId, groupId)
            .With(result => result.Deadline, deadline ?? DateTimeOffset.UtcNow.AddDays(1))
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        return homework;
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
