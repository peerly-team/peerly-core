using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetTeacherHomework;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Homeworks;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Homeworks.GetTeacherHomework;

public sealed class GetTeacherHomeworkQueryValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlyHomeworkRepository> _homeworkRepositoryMock = new();
    private readonly Mock<IReadOnlyCourseTeacherRepository> _courseTeacherRepositoryMock = new();
    private readonly Mock<IReadOnlyGroupTeacherRepository> _groupTeacherRepositoryMock = new();

    private readonly Fixture _fixture = new();
    private readonly GetTeacherHomeworkQueryValidator _validator;

    public GetTeacherHomeworkQueryValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new GetTeacherHomeworkQueryValidator(unitOfWorkFactory);
    }

    [Fact]
    public async Task ValidateAsync_CourseTeacherExists_ShouldSuccess()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherHomeworkQuery>();
        var homework = CreateHomework(query.HomeworkId);
        var courseTeacher = CreateCourseTeacher(query, homework);

        SetupHomework(query, homework);
        SetupCourseTeacherExists(courseTeacher, true);

        // Act
        var act = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        _groupTeacherRepositoryMock.Verify(
            repository => repository.ExistsAsync(It.IsAny<CourseTeacher>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_GroupTeacherExists_ShouldSuccess()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherHomeworkQuery>();
        var homework = CreateHomework(query.HomeworkId);
        var courseTeacher = CreateCourseTeacher(query, homework);

        SetupHomework(query, homework);
        SetupCourseTeacherExists(courseTeacher, false);
        SetupGroupTeacherExists(courseTeacher, true);

        // Act
        var act = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidateAsync_HomeworkNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherHomeworkQuery>();
        _homeworkRepositoryMock
            .Setup(repository => repository.GetAsync(query.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Homework?)null);

        // Act
        var act = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        VerifyAccessChecksNeverCalled();
    }

    [Fact]
    public async Task ValidateAsync_CourseTeacherAndGroupTeacherNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherHomeworkQuery>();
        var homework = CreateHomework(query.HomeworkId);
        var courseTeacher = CreateCourseTeacher(query, homework);

        SetupHomework(query, homework);
        SetupCourseTeacherExists(courseTeacher, false);
        SetupGroupTeacherExists(courseTeacher, false);

        // Act
        var act = () => _validator.ValidateAsync(query, CancellationToken.None);

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
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyCourseTeacherRepository)
            .Returns(_courseTeacherRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyGroupTeacherRepository)
            .Returns(_groupTeacherRepositoryMock.Object);

        return unitOfWorkFactoryMock.Object;
    }

    private Homework CreateHomework(HomeworkId homeworkId)
    {
        return _fixture.Build<Homework>()
            .With(result => result.Id, homeworkId)
            .Create();
    }

    private static CourseTeacher CreateCourseTeacher(GetTeacherHomeworkQuery query, Homework homework)
    {
        return new CourseTeacher
        {
            CourseId = homework.CourseId,
            TeacherId = query.TeacherId
        };
    }

    private void SetupHomework(GetTeacherHomeworkQuery query, Homework homework)
    {
        _homeworkRepositoryMock
            .Setup(repository => repository.GetAsync(query.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);
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
