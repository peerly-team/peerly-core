using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Participants.BulkAddGroupStudents;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Participants;
using Peerly.Core.Models.Students;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Participants.BulkAddGroupStudents;

public sealed class BulkAddGroupStudentsHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IStudentRepository> _studentRepositoryMock = new();
    private readonly Mock<IGroupStudentRepository> _groupStudentRepositoryMock = new();
    private readonly Mock<ICommandValidator<BulkAddGroupStudentsCommand, BulkAddGroupStudentsCommandResponse>> _validatorMock = new();
    private readonly Mock<IClock> _clockMock = new();

    private readonly Fixture _fixture = new();
    private readonly BulkAddGroupStudentsHandler _handler;
    private readonly DateTimeOffset _currentTime;

    public BulkAddGroupStudentsHandlerTests()
    {
        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.StudentRepository)
            .Returns(_studentRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.GroupStudentRepository)
            .Returns(_groupStudentRepositoryMock.Object);

        _currentTime = _fixture.Create<DateTimeOffset>();
        _clockMock
            .Setup(clock => clock.GetCurrentTime())
            .Returns(_currentTime);

        _handler = new BulkAddGroupStudentsHandler(
            _unitOfWorkFactoryMock.Object,
            _validatorMock.Object,
            _clockMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_SomeStudentsAddedAndSomeSkipped_ShouldReturnPartialResult()
    {
        // Arrange
        var addedStudentId = new StudentId(1);
        var alreadyInGroupStudentId = new StudentId(2);
        var missingStudentId = new StudentId(3);
        var command = _fixture.Build<BulkAddGroupStudentsCommand>()
            .With(item => item.StudentIds, [addedStudentId, alreadyInGroupStudentId, missingStudentId])
            .Create();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok());

        _studentRepositoryMock
            .Setup(repository => repository.ListAsync(It.IsAny<StudentFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                CreateStudent(addedStudentId),
                CreateStudent(alreadyInGroupStudentId)
            ]);
        _groupStudentRepositoryMock
            .Setup(repository => repository.ListAsync(It.IsAny<GroupStudentFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new GroupStudent
                {
                    GroupId = command.GroupId,
                    StudentId = alreadyInGroupStudentId
                }
            ]);
        _groupStudentRepositoryMock
            .Setup(repository => repository.BulkAddAsync(
                It.Is<GroupStudentBulkAddItem>(item =>
                    item.GroupId == command.GroupId
                    && item.StudentIds.SequenceEqual(new[] { addedStudentId })
                    && item.CreationTime == _currentTime),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([addedStudentId]);

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT0.Should().BeTrue();
        response.AsT0.AddedStudentIds.Should().Equal(addedStudentId);
        response.AsT0.SkippedStudents.Should().BeEquivalentTo(
        [
            new SkippedStudentInfo
            {
                Id = alreadyInGroupStudentId,
                Reason = SkippedStudentReason.AlreadyInGroup
            },
            new SkippedStudentInfo
            {
                Id = missingStudentId,
                Reason = SkippedStudentReason.NotFound
            }
        ], options => options.WithStrictOrdering());
        _groupStudentRepositoryMock.Verify(
            repository => repository.BulkAddAsync(It.IsAny<GroupStudentBulkAddItem>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_AllStudentsAlreadyInGroup_ShouldNotInsert()
    {
        // Arrange
        var firstStudentId = new StudentId(1);
        var secondStudentId = new StudentId(2);
        var command = _fixture.Build<BulkAddGroupStudentsCommand>()
            .With(item => item.StudentIds, [firstStudentId, secondStudentId])
            .Create();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok());
        _studentRepositoryMock
            .Setup(repository => repository.ListAsync(It.IsAny<StudentFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateStudent(firstStudentId), CreateStudent(secondStudentId)]);
        _groupStudentRepositoryMock
            .Setup(repository => repository.ListAsync(It.IsAny<GroupStudentFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new GroupStudent { GroupId = command.GroupId, StudentId = firstStudentId },
                new GroupStudent { GroupId = command.GroupId, StudentId = secondStudentId }
            ]);

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT0.Should().BeTrue();
        response.AsT0.AddedStudentIds.Should().BeEmpty();
        response.AsT0.SkippedStudents.Should().AllSatisfy(
            item => item.Reason.Should().Be(SkippedStudentReason.AlreadyInGroup));
        _groupStudentRepositoryMock.Verify(
            repository => repository.BulkAddAsync(It.IsAny<GroupStudentBulkAddItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatorReturnsPermissionDenied_ShouldBeOtherError()
    {
        // Arrange
        var command = _fixture.Create<BulkAddGroupStudentsCommand>();
        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtherError.PermissionDenied());

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT2.Should().BeTrue();
        response.AsT2.Type.Should().Be(ErrorType.PermissionDenied);
        _studentRepositoryMock.Verify(
            repository => repository.ListAsync(It.IsAny<StudentFilter>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _groupStudentRepositoryMock.Verify(
            repository => repository.BulkAddAsync(It.IsAny<GroupStudentBulkAddItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private Student CreateStudent(StudentId studentId)
    {
        return _fixture.Build<Student>()
            .With(student => student.Id, studentId)
            .Create();
    }
}
