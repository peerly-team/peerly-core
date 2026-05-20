using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetStudentHomework;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Homeworks.GetStudentHomework;

public sealed class GetStudentHomeworkQueryValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlyHomeworkRepository> _homeworkRepositoryMock = new();
    private readonly Mock<IReadOnlyGroupStudentRepository> _groupStudentRepositoryMock = new();

    private readonly Fixture _fixture = new();
    private readonly GetStudentHomeworkQueryValidator _validator;

    public GetStudentHomeworkQueryValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new GetStudentHomeworkQueryValidator(unitOfWorkFactory);
    }

    [Fact]
    public async Task ValidateAsync_GroupHomeworkAndGroupStudentExists_ShouldSuccess()
    {
        // Arrange
        var query = _fixture.Create<GetStudentHomeworkQuery>();
        var groupId = _fixture.Create<GroupId>();
        var homework = CreateHomework(query.HomeworkId, HomeworkStatus.Published, groupId);
        SetupHomework(query, homework);
        SetupGroupStudentExists(groupId, query.StudentId, true);

        // Act
        Func<Task> act = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        _groupStudentRepositoryMock.Verify(
            repository => repository.ExistsAsync(
                It.Is<GroupStudent>(parameter =>
                    parameter.GroupId == groupId
                    && parameter.StudentId == query.StudentId),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _groupStudentRepositoryMock.Verify(
            repository => repository.ExistsAsync(It.IsAny<CourseStudent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_CourseHomeworkAndCourseStudentExists_ShouldSuccess()
    {
        // Arrange
        var query = _fixture.Create<GetStudentHomeworkQuery>();
        var homework = CreateHomework(query.HomeworkId, HomeworkStatus.Published, null);
        SetupHomework(query, homework);
        SetupCourseStudentExists(homework.CourseId, query.StudentId, true);

        // Act
        Func<Task> act = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        _groupStudentRepositoryMock.Verify(
            repository => repository.ExistsAsync(
                It.Is<CourseStudent>(parameter =>
                    parameter.CourseId == homework.CourseId
                    && parameter.StudentId == query.StudentId),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _groupStudentRepositoryMock.Verify(
            repository => repository.ExistsAsync(It.IsAny<GroupStudent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_HomeworkNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetStudentHomeworkQuery>();
        _homeworkRepositoryMock
            .Setup(repository => repository.GetAsync(query.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Homework?)null);

        // Act
        Func<Task> act = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        VerifyAccessChecksNeverCalled();
    }

    [Fact]
    public async Task ValidateAsync_DraftHomework_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetStudentHomeworkQuery>();
        var homework = CreateHomework(query.HomeworkId, HomeworkStatus.Draft, null);
        SetupHomework(query, homework);

        // Act
        Func<Task> act = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        VerifyAccessChecksNeverCalled();
    }

    [Fact]
    public async Task ValidateAsync_GroupStudentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetStudentHomeworkQuery>();
        var groupId = _fixture.Create<GroupId>();
        var homework = CreateHomework(query.HomeworkId, HomeworkStatus.Published, groupId);
        SetupHomework(query, homework);
        SetupGroupStudentExists(groupId, query.StudentId, false);

        // Act
        Func<Task> act = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ValidateAsync_CourseStudentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetStudentHomeworkQuery>();
        var homework = CreateHomework(query.HomeworkId, HomeworkStatus.Published, null);
        SetupHomework(query, homework);
        SetupCourseStudentExists(homework.CourseId, query.StudentId, false);

        // Act
        Func<Task> act = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var unitOfWorkFactoryMock = new Mock<ICommonUnitOfWorkFactory>();
        unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyHomeworkRepository)
            .Returns(_homeworkRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyGroupStudentRepository)
            .Returns(_groupStudentRepositoryMock.Object);

        return unitOfWorkFactoryMock.Object;
    }

    private Homework CreateHomework(HomeworkId homeworkId, HomeworkStatus homeworkStatus, GroupId? groupId)
    {
        return _fixture.Build<Homework>()
            .With(result => result.Id, homeworkId)
            .With(result => result.Status, homeworkStatus)
            .With(result => result.GroupId, groupId)
            .Create();
    }

    private void SetupHomework(GetStudentHomeworkQuery query, Homework homework)
    {
        _homeworkRepositoryMock
            .Setup(repository => repository.GetAsync(query.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);
    }

    private void SetupGroupStudentExists(GroupId groupId, StudentId studentId, bool exists)
    {
        _groupStudentRepositoryMock
            .Setup(repository => repository.ExistsAsync(
                It.Is<GroupStudent>(parameter =>
                    parameter.GroupId == groupId
                    && parameter.StudentId == studentId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    private void SetupCourseStudentExists(CourseId courseId, StudentId studentId, bool exists)
    {
        _groupStudentRepositoryMock
            .Setup(repository => repository.ExistsAsync(
                It.Is<CourseStudent>(parameter =>
                    parameter.CourseId == courseId
                    && parameter.StudentId == studentId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    private void VerifyAccessChecksNeverCalled()
    {
        _groupStudentRepositoryMock.Verify(
            repository => repository.ExistsAsync(It.IsAny<GroupStudent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _groupStudentRepositoryMock.Verify(
            repository => repository.ExistsAsync(It.IsAny<CourseStudent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
