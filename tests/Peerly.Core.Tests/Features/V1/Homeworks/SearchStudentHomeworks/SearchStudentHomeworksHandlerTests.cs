using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchStudentHomeworks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Pagination;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Homeworks.SearchStudentHomeworks;

public sealed class SearchStudentHomeworksHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Fixture _fixture = new();
    private readonly SearchStudentHomeworksHandler _handler;

    public SearchStudentHomeworksHandlerTests()
    {
        SetupUnitOfWorkFactory();
        _handler = new SearchStudentHomeworksHandler(_unitOfWorkFactoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_CourseIdsEmpty_ShouldReturnEmptyHomeworks()
    {
        // Arrange
        var query = CreateQuery([HomeworkStatus.Published]);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupRepository.ListCourseIdsAsync(query.StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var queryResponse = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        queryResponse.StudentHomeworks.Should().BeEmpty();
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyGroupRepository.ListCourseIdsAsync(query.StudentId, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyGroupRepository.ListGroupIdsAsync(
                It.IsAny<StudentId>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyHomeworkRepository.SearchStudentHomeworkInfosAsync(
                It.IsAny<StudentHomeworkSearchFilter>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_CourseAndGroupIdsExist_ShouldSearchHomeworksAndReturnResults()
    {
        // Arrange
        var query = CreateQuery([HomeworkStatus.Published, HomeworkStatus.Reviewing]);
        var courseIds = new[] { (CourseId)1, (CourseId)2 };
        var groupIds = new[] { (GroupId)10, (GroupId)20 };
        var homeworks = _fixture.CreateMany<StudentHomeworkInfo>().ToArray();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupRepository.ListCourseIdsAsync(query.StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseIds);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupRepository.ListGroupIdsAsync(query.StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(groupIds);

        var expectedFilter = new StudentHomeworkSearchFilter
        {
            StudentId = query.StudentId,
            CourseIds = courseIds,
            GroupIds = groupIds,
            HomeworkStatuses = query.Filter.HomeworkStatuses
        };
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyHomeworkRepository.SearchStudentHomeworkInfosAsync(
                It.Is<StudentHomeworkSearchFilter>(filter => FiltersAreEquivalent(filter, expectedFilter)),
                query.PaginationInfo,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(homeworks);

        // Act
        var queryResponse = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        queryResponse.StudentHomeworks.Should().BeEquivalentTo(homeworks);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyHomeworkRepository.SearchStudentHomeworkInfosAsync(
                It.Is<StudentHomeworkSearchFilter>(filter => FiltersAreEquivalent(filter, expectedFilter)),
                query.PaginationInfo,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyStatusesFilter_ShouldSearchVisibleStatuses()
    {
        // Arrange
        var query = CreateQuery([]);
        var courseIds = new[] { (CourseId)1 };
        var groupIds = new[] { (GroupId)10 };
        var expectedFilter = new StudentHomeworkSearchFilter
        {
            StudentId = query.StudentId,
            CourseIds = courseIds,
            GroupIds = groupIds,
            HomeworkStatuses =
            [
                HomeworkStatus.Published,
                HomeworkStatus.Reviewing,
                HomeworkStatus.Confirmation,
                HomeworkStatus.Finished
            ]
        };

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupRepository.ListCourseIdsAsync(query.StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseIds);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupRepository.ListGroupIdsAsync(query.StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(groupIds);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyHomeworkRepository.SearchStudentHomeworkInfosAsync(
                It.Is<StudentHomeworkSearchFilter>(filter => FiltersAreEquivalent(filter, expectedFilter)),
                query.PaginationInfo,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyHomeworkRepository.SearchStudentHomeworkInfosAsync(
                It.Is<StudentHomeworkSearchFilter>(filter => FiltersAreEquivalent(filter, expectedFilter)),
                query.PaginationInfo,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private SearchStudentHomeworksQuery CreateQuery(IReadOnlyCollection<HomeworkStatus> homeworkStatuses)
    {
        return new SearchStudentHomeworksQuery
        {
            StudentId = _fixture.Create<StudentId>(),
            Filter = new SearchStudentHomeworksQueryFilter { HomeworkStatuses = homeworkStatuses },
            PaginationInfo = new PaginationInfo
            {
                Offset = 0,
                PageSize = 10
            }
        };
    }

    private void SetupUnitOfWorkFactory()
    {
        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);
    }

    private static bool FiltersAreEquivalent(StudentHomeworkSearchFilter actual, StudentHomeworkSearchFilter expected)
    {
        return actual.StudentId == expected.StudentId
               && CollectionsAreEquivalent(actual.CourseIds, expected.CourseIds)
               && CollectionsAreEquivalent(actual.GroupIds, expected.GroupIds)
               && CollectionsAreEquivalent(actual.HomeworkStatuses, expected.HomeworkStatuses);
    }

    private static bool CollectionsAreEquivalent<T>(IReadOnlyCollection<T> actual, IReadOnlyCollection<T> expected)
    {
        return !actual.Except(expected).Any() && !expected.Except(actual).Any();
    }
}
