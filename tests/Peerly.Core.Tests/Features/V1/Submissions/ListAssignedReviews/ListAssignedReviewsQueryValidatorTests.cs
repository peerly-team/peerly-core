using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.ListAssignedReviews;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.ListAssignedReviews;

public sealed class ListAssignedReviewsQueryValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlyHomeworkRepository> _homeworkRepositoryMock = new();
    private readonly Mock<IReadOnlyGroupStudentRepository> _groupStudentRepositoryMock = new();
    private readonly Fixture _fixture = new();
    private readonly ListAssignedReviewsQueryValidator _validator;

    public ListAssignedReviewsQueryValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new ListAssignedReviewsQueryValidator(unitOfWorkFactory);
    }

    [Fact]
    public async Task ValidateAsync_GroupStudentExists_ShouldSuccess()
    {
        // Arrange
        var query = _fixture.Create<ListAssignedReviewsQuery>();
        var homework = SetupHomework(query.HomeworkId, HomeworkStatus.Reviewing, _fixture.Create<GroupId>());
        var groupStudent = query.ToGroupStudent(homework.GroupId!.Value);

        SetupGroupStudentExists(groupStudent, true);

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
        _groupStudentRepositoryMock.Verify(
            repository => repository.ExistsAsync(It.IsAny<CourseStudent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_CourseStudentExists_ShouldSuccess()
    {
        // Arrange
        var query = _fixture.Create<ListAssignedReviewsQuery>();
        var homework = SetupHomework(query.HomeworkId, HomeworkStatus.Reviewing, groupId: null);
        var courseStudent = query.ToCourseStudent(homework.CourseId);

        SetupCourseStudentExists(courseStudent, true);

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
        _groupStudentRepositoryMock.Verify(
            repository => repository.ExistsAsync(It.IsAny<GroupStudent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_HomeworkNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<ListAssignedReviewsQuery>();
        _homeworkRepositoryMock
            .Setup(repository => repository.GetAsync(query.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Homework?)null);

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
        VerifyStudentAccessChecksNeverCalled();
    }

    [Theory]
    [InlineData(HomeworkStatus.Draft)]
    [InlineData(HomeworkStatus.Published)]
    [InlineData(HomeworkStatus.Confirmation)]
    [InlineData(HomeworkStatus.Finished)]
    public async Task ValidateAsync_HomeworkNotInReviewingStatus_ShouldThrowNotFoundException(HomeworkStatus status)
    {
        // Arrange
        var query = _fixture.Create<ListAssignedReviewsQuery>();
        SetupHomework(query.HomeworkId, status, _fixture.Create<GroupId>());

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
        VerifyStudentAccessChecksNeverCalled();
    }

    [Fact]
    public async Task ValidateAsync_GroupStudentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<ListAssignedReviewsQuery>();
        var homework = SetupHomework(query.HomeworkId, HomeworkStatus.Reviewing, _fixture.Create<GroupId>());
        var groupStudent = query.ToGroupStudent(homework.GroupId!.Value);

        SetupGroupStudentExists(groupStudent, false);

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ValidateAsync_CourseStudentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<ListAssignedReviewsQuery>();
        var homework = SetupHomework(query.HomeworkId, HomeworkStatus.Reviewing, groupId: null);
        var courseStudent = query.ToCourseStudent(homework.CourseId);

        SetupCourseStudentExists(courseStudent, false);

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
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

    private Homework SetupHomework(HomeworkId homeworkId, HomeworkStatus status, GroupId? groupId)
    {
        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, homeworkId)
            .With(result => result.Status, status)
            .With(result => result.GroupId, groupId)
            .Create();
        _homeworkRepositoryMock
            .Setup(repository => repository.GetAsync(homeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        return homework;
    }

    private void SetupGroupStudentExists(GroupStudent groupStudent, bool exists)
    {
        _groupStudentRepositoryMock
            .Setup(repository => repository.ExistsAsync(groupStudent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    private void SetupCourseStudentExists(CourseStudent courseStudent, bool exists)
    {
        _groupStudentRepositoryMock
            .Setup(repository => repository.ExistsAsync(courseStudent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    private void VerifyStudentAccessChecksNeverCalled()
    {
        _groupStudentRepositoryMock.Verify(
            repository => repository.ExistsAsync(It.IsAny<GroupStudent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _groupStudentRepositoryMock.Verify(
            repository => repository.ExistsAsync(It.IsAny<CourseStudent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
