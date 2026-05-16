using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.ListStudentCourseHomeworks;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Homeworks;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Homeworks.ListStudentCourseHomeworks;

public sealed class ListStudentCourseHomeworksHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlyHomeworkRepository> _homeworkRepositoryMock = new();
    private readonly Fixture _fixture = new();
    private readonly ListStudentCourseHomeworksHandler _handler;

    public ListStudentCourseHomeworksHandlerTests()
    {
        SetupUnitOfWorkFactory();
        _handler = new ListStudentCourseHomeworksHandler(_unitOfWorkFactoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_CourseStudentExists_ShouldReturnStudentHomeworks()
    {
        // Arrange
        var query = _fixture.Create<ListStudentCourseHomeworksQuery>();
        var homeworks = _fixture.CreateMany<StudentHomeworkInfo>().ToArray();

        _homeworkRepositoryMock
            .Setup(repository => repository.ListStudentHomeworkInfosAsync(
                It.Is<CourseStudent>(courseStudent => courseStudent.CourseId == query.CourseId && courseStudent.StudentId == query.StudentId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(homeworks);

        // Act
        var queryResponse = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        queryResponse.StudentHomeworks.Should().BeEquivalentTo(homeworks);
        _unitOfWorkFactoryMock.Verify(
            factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _homeworkRepositoryMock.Verify(
            repository => repository.ListStudentHomeworkInfosAsync(
                It.Is<CourseStudent>(courseStudent => courseStudent.CourseId == query.CourseId && courseStudent.StudentId == query.StudentId),
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
