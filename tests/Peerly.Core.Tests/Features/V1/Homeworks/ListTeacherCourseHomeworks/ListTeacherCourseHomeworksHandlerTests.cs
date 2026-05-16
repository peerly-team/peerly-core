using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.ListTeacherCourseHomeworks;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Homeworks;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Homeworks.ListTeacherCourseHomeworks;

public sealed class ListTeacherCourseHomeworksHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlyHomeworkRepository> _homeworkRepositoryMock = new();
    private readonly Fixture _fixture = new();
    private readonly ListTeacherCourseHomeworksHandler _handler;

    public ListTeacherCourseHomeworksHandlerTests()
    {
        SetupUnitOfWorkFactory();
        _handler = new ListTeacherCourseHomeworksHandler(_unitOfWorkFactoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_CourseTeacherExists_ShouldReturnTeacherHomeworks()
    {
        // Arrange
        var query = _fixture.Create<ListTeacherCourseHomeworksQuery>();
        var homeworks = _fixture.CreateMany<TeacherHomeworkInfo>().ToArray();

        _homeworkRepositoryMock
            .Setup(repository => repository.ListTeacherHomeworkInfosAsync(
                It.Is<CourseTeacher>(courseTeacher => courseTeacher.CourseId == query.CourseId && courseTeacher.TeacherId == query.TeacherId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(homeworks);

        // Act
        var queryResponse = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        queryResponse.Homeworks.Should().BeEquivalentTo(homeworks);
        _unitOfWorkFactoryMock.Verify(
            factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _homeworkRepositoryMock.Verify(
            repository => repository.ListTeacherHomeworkInfosAsync(
                It.Is<CourseTeacher>(courseTeacher => courseTeacher.CourseId == query.CourseId && courseTeacher.TeacherId == query.TeacherId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void SetupUnitOfWorkFactory()
    {
        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyHomeworkRepository)
            .Returns(_homeworkRepositoryMock.Object);
    }
}
