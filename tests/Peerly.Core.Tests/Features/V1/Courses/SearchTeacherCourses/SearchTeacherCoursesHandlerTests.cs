using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Courses.SearchTeacherCourses;
using Peerly.Core.ApplicationServices.Features.V1.Courses.Shared.SearchCourses;
using Peerly.Core.Models.Courses;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Courses.SearchTeacherCourses;

public sealed class SearchTeacherCoursesHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();

    private readonly Fixture _fixture = new();
    private readonly SearchTeacherCoursesHandler _handler;

    public SearchTeacherCoursesHandlerTests()
    {
        SetupUnitOfWorkFactory();
        _handler = new SearchTeacherCoursesHandler(_unitOfWorkFactoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_CourseIdsEmpty_ShouldReturnEmptyCourses()
    {
        // Arrange
        var searchCoursesQueryFilter = _fixture.Build<SearchCoursesQueryFilter>()
            .With(result => result.CourseStatuses, [CourseStatus.InProgress, CourseStatus.Finished])
            .Create();
        var query = _fixture.Build<SearchTeacherCoursesQuery>()
            .With(query => query.Filter, searchCoursesQueryFilter)
            .Create();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseTeacherRepository.ListCourseIdAsync(query.TeacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupRepository.ListCourseIdAsync(query.TeacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var queryResponse = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        queryResponse.Courses.Should().BeEmpty();
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyCourseTeacherRepository.ListCourseIdAsync(query.TeacherId, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyGroupRepository.ListCourseIdAsync(query.TeacherId, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyCourseRepository.ListAsync(
                It.IsAny<CourseFilter>(),
                query.PaginationInfo,
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_CourseIdsExist_ShouldReturnNotDeletedCourses()
    {
        // Arrange
        var searchCoursesQueryFilter = _fixture.Build<SearchCoursesQueryFilter>()
            .With(result => result.CourseStatuses, [CourseStatus.InProgress, CourseStatus.Finished, CourseStatus.Deleted])
            .Create();
        var query = _fixture.Build<SearchTeacherCoursesQuery>()
            .With(query => query.Filter, searchCoursesQueryFilter)
            .Create();

        var finishedCourse = _fixture.Build<Course>()
            .With(result => result.Status, CourseStatus.Finished)
            .Create();
        var deletedCourse = _fixture.Build<Course>()
            .With(result => result.Status, CourseStatus.Deleted)
            .Create();
        var inProgressCourse = _fixture.Build<Course>()
            .With(result => result.Status, CourseStatus.InProgress)
            .Create();

        var courseIds = new[] { finishedCourse.Id };
        var groupCourseIds = new[] { deletedCourse.Id, inProgressCourse.Id };
        var generalCourseIds = courseIds.Concat(groupCourseIds).ToArray();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseTeacherRepository.ListCourseIdAsync(query.TeacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseIds);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupRepository.ListCourseIdAsync(query.TeacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(groupCourseIds);

        var courseFilter = new CourseFilter
        {
            CourseIds = generalCourseIds,
            CourseStatuses = query.Filter.CourseStatuses
        };
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseRepository.ListAsync(
                It.Is<CourseFilter>(parameter => CourseFiltersAreEquivalent(parameter, courseFilter)),
                query.PaginationInfo,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([finishedCourse, deletedCourse, inProgressCourse]);

        // Act
        var queryResponse = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        queryResponse.Courses.Should().BeEquivalentTo([finishedCourse, inProgressCourse]);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyCourseTeacherRepository.ListCourseIdAsync(query.TeacherId, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyGroupRepository.ListCourseIdAsync(query.TeacherId, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyCourseRepository.ListAsync(
                It.Is<CourseFilter>(parameter => CourseFiltersAreEquivalent(parameter, courseFilter)),
                query.PaginationInfo,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void SetupUnitOfWorkFactory()
    {
        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);
    }

    private static bool CourseFiltersAreEquivalent(CourseFilter actual, CourseFilter expected)
    {
        return !(actual.CourseIds.Except(expected.CourseIds).Any()
               || expected.CourseIds.Except(actual.CourseIds).Any()
               || actual.CourseStatuses.Except(expected.CourseStatuses).Any()
               || expected.CourseStatuses.Except(actual.CourseStatuses).Any());
    }
}
