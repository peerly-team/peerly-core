using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Courses.GetTeacherCourse;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Courses;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Courses.GetTeacherCourse;

public sealed class GetTeacherCourseQueryValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();

    private readonly Fixture _fixture = new();
    private readonly GetTeacherCourseQueryValidator _validator;

    public GetTeacherCourseQueryValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new GetTeacherCourseQueryValidator(unitOfWorkFactory);
    }

    [Fact]
    public async Task ValidateAsync_CourseTeacherAndCourseExist_ShouldSuccess()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherCourseQuery>();
        var courseTeacher = _fixture.Build<CourseTeacher>()
            .With(result => result.CourseId, query.CourseId)
            .With(result => result.TeacherId, query.TeacherId)
            .Create();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseRepository.ExistsAsync(query.CourseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyGroupTeacherRepository.ExistsAsync(It.IsAny<CourseTeacher>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_GroupTeacherAndCourseExist_ShouldSuccess()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherCourseQuery>();
        var courseTeacher = _fixture.Build<CourseTeacher>()
            .With(result => result.CourseId, query.CourseId)
            .With(result => result.TeacherId, query.TeacherId)
            .Create();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupTeacherRepository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseRepository.ExistsAsync(query.CourseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidateAsync_CourseTeacherAndGroupTeacherNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherCourseQuery>();
        var courseTeacher = _fixture.Build<CourseTeacher>()
            .With(result => result.CourseId, query.CourseId)
            .With(result => result.TeacherId, query.TeacherId)
            .Create();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupTeacherRepository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyCourseRepository.ExistsAsync(query.CourseId, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_CourseNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherCourseQuery>();
        var courseTeacher = _fixture.Build<CourseTeacher>()
            .With(result => result.CourseId, query.CourseId)
            .With(result => result.TeacherId, query.TeacherId)
            .Create();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseRepository.ExistsAsync(query.CourseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

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

        return unitOfWorkFactoryMock.Object;
    }
}
