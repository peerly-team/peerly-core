using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchTeacherHomeworks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Pagination;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Homeworks.SearchTeacherHomeworks;

public sealed class SearchTeacherHomeworksHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Fixture _fixture = new();
    private readonly SearchTeacherHomeworksHandler _handler;

    public SearchTeacherHomeworksHandlerTests()
    {
        SetupUnitOfWorkFactory();
        _handler = new SearchTeacherHomeworksHandler(_unitOfWorkFactoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_CourseAndGroupIdsEmpty_ShouldReturnEmptyHomeworks()
    {
        // Arrange
        var query = CreateQuery(_fixture.CreateMany<HomeworkStatus>().ToArray());

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseTeacherRepository.ListCourseIdsAsync(query.TeacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupTeacherRepository.ListGroupIdsAsync(query.TeacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var queryResponse = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        queryResponse.TeacherHomeworks.Should().BeEmpty();
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyCourseTeacherRepository.ListCourseIdsAsync(query.TeacherId, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyGroupTeacherRepository.ListGroupIdsAsync(query.TeacherId, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyHomeworkRepository.SearchTeacherHomeworkInfosAsync(
                It.IsAny<TeacherHomeworkSearchFilter>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_CourseAndGroupIdsExist_ShouldSearchHomeworksAndReturnResults()
    {
        // Arrange
        var query = CreateQuery(_fixture.CreateMany<HomeworkStatus>().ToArray());
        var courseIds = _fixture.CreateMany<CourseId>().ToArray();
        var groupIds = _fixture.CreateMany<GroupId>().ToArray();
        var homeworks = _fixture.CreateMany<TeacherHomeworkInfo>().ToArray();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseTeacherRepository.ListCourseIdsAsync(query.TeacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseIds);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupTeacherRepository.ListGroupIdsAsync(query.TeacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(groupIds);

        var expectedFilter = new TeacherHomeworkSearchFilter
        {
            CourseIds = courseIds,
            GroupIds = groupIds,
            HomeworkStatuses = query.Filter.HomeworkStatuses
        };
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyHomeworkRepository.SearchTeacherHomeworkInfosAsync(
                It.Is<TeacherHomeworkSearchFilter>(filter => FiltersAreEquivalent(filter, expectedFilter)),
                query.PaginationInfo,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(homeworks);

        // Act
        var queryResponse = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        queryResponse.TeacherHomeworks.Should().BeEquivalentTo(homeworks);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyHomeworkRepository.SearchTeacherHomeworkInfosAsync(
                It.Is<TeacherHomeworkSearchFilter>(filter => FiltersAreEquivalent(filter, expectedFilter)),
                query.PaginationInfo,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyStatusesFilter_ShouldSearchWithEmptyStatuses()
    {
        // Arrange
        var query = CreateQuery([]);
        var courseIds = _fixture.CreateMany<CourseId>().ToArray();
        var groupIds = _fixture.CreateMany<GroupId>().ToArray();
        var expectedFilter = new TeacherHomeworkSearchFilter
        {
            CourseIds = courseIds,
            GroupIds = groupIds,
            HomeworkStatuses = query.Filter.HomeworkStatuses
        };

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseTeacherRepository.ListCourseIdsAsync(query.TeacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseIds);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupTeacherRepository.ListGroupIdsAsync(query.TeacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(groupIds);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyHomeworkRepository.SearchTeacherHomeworkInfosAsync(
                It.Is<TeacherHomeworkSearchFilter>(filter => FiltersAreEquivalent(filter, expectedFilter)),
                query.PaginationInfo,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyHomeworkRepository.SearchTeacherHomeworkInfosAsync(
                It.Is<TeacherHomeworkSearchFilter>(filter => FiltersAreEquivalent(filter, expectedFilter)),
                query.PaginationInfo,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private SearchTeacherHomeworksQuery CreateQuery(IReadOnlyCollection<HomeworkStatus> homeworkStatuses)
    {
        return new SearchTeacherHomeworksQuery
        {
            TeacherId = _fixture.Create<TeacherId>(),
            Filter = new SearchTeacherHomeworksQueryFilter { HomeworkStatuses = homeworkStatuses },
            PaginationInfo = _fixture.Create<PaginationInfo>()
        };
    }

    private void SetupUnitOfWorkFactory()
    {
        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);
    }

    private static bool FiltersAreEquivalent(TeacherHomeworkSearchFilter actual, TeacherHomeworkSearchFilter expected)
    {
        return CollectionsAreEquivalent(actual.CourseIds, expected.CourseIds)
               && CollectionsAreEquivalent(actual.GroupIds, expected.GroupIds)
               && CollectionsAreEquivalent(actual.HomeworkStatuses, expected.HomeworkStatuses);
    }

    private static bool CollectionsAreEquivalent<T>(IReadOnlyCollection<T> actual, IReadOnlyCollection<T> expected)
    {
        return !actual.Except(expected).Any() && !expected.Except(actual).Any();
    }
}
