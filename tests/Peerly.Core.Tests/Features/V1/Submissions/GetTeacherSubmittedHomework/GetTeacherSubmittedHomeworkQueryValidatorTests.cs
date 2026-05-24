using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetTeacherSubmittedHomework;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Submissions;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.GetTeacherSubmittedHomework;

public sealed class GetTeacherSubmittedHomeworkQueryValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlySubmittedHomeworkRepository> _submittedHomeworkRepositoryMock = new();
    private readonly Mock<IReadOnlyHomeworkRepository> _homeworkRepositoryMock = new();
    private readonly Mock<IReadOnlyCourseTeacherRepository> _courseTeacherRepositoryMock = new();
    private readonly Mock<IReadOnlyGroupTeacherRepository> _groupTeacherRepositoryMock = new();
    private readonly Fixture _fixture = new();
    private readonly GetTeacherSubmittedHomeworkQueryValidator _validator;

    public GetTeacherSubmittedHomeworkQueryValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new GetTeacherSubmittedHomeworkQueryValidator(unitOfWorkFactory);
    }

    [Fact]
    public async Task ValidateAsync_CourseTeacherExists_ShouldSuccess()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherSubmittedHomeworkQuery>();
        var submittedHomework = SetupSubmittedHomework(query.SubmittedHomeworkId);
        var homework = SetupHomework(submittedHomework.HomeworkId, HomeworkStatus.Finished);
        var courseTeacher = query.ToCourseTeacher(homework.CourseId);

        SetupCourseTeacherExists(courseTeacher, true);

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
        _groupTeacherRepositoryMock.Verify(
            repository => repository.ExistsAsync(It.IsAny<CourseTeacher>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_GroupTeacherExists_ShouldSuccess()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherSubmittedHomeworkQuery>();
        var submittedHomework = SetupSubmittedHomework(query.SubmittedHomeworkId);
        var homework = SetupHomework(submittedHomework.HomeworkId, HomeworkStatus.Confirmation);
        var courseTeacher = query.ToCourseTeacher(homework.CourseId);

        SetupCourseTeacherExists(courseTeacher, false);
        SetupGroupTeacherExists(courseTeacher, true);

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidateAsync_SubmittedHomeworkNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherSubmittedHomeworkQuery>();
        _submittedHomeworkRepositoryMock
            .Setup(repository => repository.GetAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubmittedHomework?)null);

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
        _homeworkRepositoryMock.Verify(
            repository => repository.GetAsync(It.IsAny<HomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
        VerifyAccessChecksNeverCalled();
    }

    [Fact]
    public async Task ValidateAsync_LinkedHomeworkNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherSubmittedHomeworkQuery>();
        var submittedHomework = SetupSubmittedHomework(query.SubmittedHomeworkId);
        _homeworkRepositoryMock
            .Setup(repository => repository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Homework?)null);

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
        VerifyAccessChecksNeverCalled();
    }

    [Theory]
    [InlineData(HomeworkStatus.Draft)]
    [InlineData(HomeworkStatus.Published)]
    public async Task ValidateAsync_HomeworkNotVisibleToTeacher_ShouldThrowNotFoundException(HomeworkStatus status)
    {
        // Arrange
        var query = _fixture.Create<GetTeacherSubmittedHomeworkQuery>();
        var submittedHomework = SetupSubmittedHomework(query.SubmittedHomeworkId);
        SetupHomework(submittedHomework.HomeworkId, status);

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
        VerifyAccessChecksNeverCalled();
    }

    [Fact]
    public async Task ValidateAsync_CourseTeacherAndGroupTeacherNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherSubmittedHomeworkQuery>();
        var submittedHomework = SetupSubmittedHomework(query.SubmittedHomeworkId);
        var homework = SetupHomework(submittedHomework.HomeworkId, HomeworkStatus.Reviewing);
        var courseTeacher = query.ToCourseTeacher(homework.CourseId);

        SetupCourseTeacherExists(courseTeacher, false);
        SetupGroupTeacherExists(courseTeacher, false);

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
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySubmittedHomeworkRepository)
            .Returns(_submittedHomeworkRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyHomeworkRepository)
            .Returns(_homeworkRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyCourseTeacherRepository)
            .Returns(_courseTeacherRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyGroupTeacherRepository)
            .Returns(_groupTeacherRepositoryMock.Object);

        return unitOfWorkFactoryMock.Object;
    }

    private SubmittedHomework SetupSubmittedHomework(SubmittedHomeworkId submittedHomeworkId)
    {
        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(result => result.Id, submittedHomeworkId)
            .Create();
        _submittedHomeworkRepositoryMock
            .Setup(repository => repository.GetAsync(submittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomework);

        return submittedHomework;
    }

    private Homework SetupHomework(HomeworkId homeworkId, HomeworkStatus status)
    {
        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, homeworkId)
            .With(result => result.Status, status)
            .Create();
        _homeworkRepositoryMock
            .Setup(repository => repository.GetAsync(homeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        return homework;
    }

    private void SetupCourseTeacherExists(CourseTeacher courseTeacher, bool exists)
    {
        _courseTeacherRepositoryMock
            .Setup(repository => repository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    private void SetupGroupTeacherExists(CourseTeacher courseTeacher, bool exists)
    {
        _groupTeacherRepositoryMock
            .Setup(repository => repository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    private void VerifyAccessChecksNeverCalled()
    {
        _courseTeacherRepositoryMock.Verify(
            repository => repository.ExistsAsync(It.IsAny<CourseTeacher>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _groupTeacherRepositoryMock.Verify(
            repository => repository.ExistsAsync(It.IsAny<CourseTeacher>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
