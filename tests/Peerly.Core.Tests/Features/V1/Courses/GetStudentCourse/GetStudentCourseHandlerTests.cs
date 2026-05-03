using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Courses.GetStudentCourse;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Courses.GetStudentCourse;

public sealed class GetStudentCourseHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IQueryValidator<GetStudentCourseQuery, GetStudentCourseQueryResponse>> _validatorMock = new();

    private readonly Fixture _fixture = new();
    private readonly GetStudentCourseHandler _handler;

    public GetStudentCourseHandlerTests()
    {
        SetupUnitOfWorkFactory();
        _handler = new GetStudentCourseHandler(_unitOfWorkFactoryMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationSuccess_ShouldReturnCourseInfo()
    {
        // Arrange
        var query = _fixture.Create<GetStudentCourseQuery>();

        var course = _fixture.Build<Course>()
            .With(result => result.Id, query.CourseId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseRepository.GetAsync(query.CourseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        var homeworkCount = _fixture.Create<int>();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyHomeworkRepository.GetHomeworkCountAsync(query.CourseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homeworkCount);

        var groups = new[]
        {
            _fixture.Build<Group>()
                .With(result => result.CourseId, query.CourseId)
                .With(result => result.StudentCount, _fixture.Create<int>())
                .Create(),
            _fixture.Build<Group>()
                .With(result => result.CourseId, query.CourseId)
                .With(result => result.StudentCount, _fixture.Create<int>())
                .Create()
        };
        var groupFilter = GroupFilter.Empty() with { CourseIds = [query.CourseId] };
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupRepository.ListAsync(
                It.Is<GroupFilter>(parameter => !(parameter.CourseIds.Except(groupFilter.CourseIds).Any() || parameter.GroupIds.Except(groupFilter.GroupIds).Any())),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(groups);

        // Act
        var queryResponse = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        queryResponse.Course.Should().BeEquivalentTo(course);
        queryResponse.HomeworkCount.Should().Be(homeworkCount);
        queryResponse.StudentCount.Should().Be(groups.Sum(group => group.StudentCount));
        _validatorMock.Verify(
            validator => validator.ValidateAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyCourseRepository.GetAsync(query.CourseId, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyHomeworkRepository.GetHomeworkCountAsync(query.CourseId, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyGroupRepository.ListAsync(
                It.Is<GroupFilter>(parameter => !(parameter.CourseIds.Except(groupFilter.CourseIds).Any() || parameter.GroupIds.Except(groupFilter.GroupIds).Any())),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationFailed_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetStudentCourseQuery>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

        // Act
        var action = () => _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkFactoryMock.Verify(
            factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupUnitOfWorkFactory()
    {
        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);
    }
}
