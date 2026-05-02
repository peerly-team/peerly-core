using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Courses.GetStudentCourse;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Courses;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Courses.GetStudentCourse;

public sealed class GetStudentCourseQueryValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();

    private readonly Fixture _fixture = new();
    private readonly GetStudentCourseQueryValidator _validator;

    public GetStudentCourseQueryValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new GetStudentCourseQueryValidator(unitOfWorkFactory);
    }

    [Fact]
    public async Task ValidateAsync_CourseAndCourseStudentExist_ShouldSuccess()
    {
        // Arrange
        var query = _fixture.Create<GetStudentCourseQuery>();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseRepository.ExistsAsync(query.CourseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var courseStudent = _fixture.Build<CourseStudent>()
            .With(result => result.CourseId, query.CourseId)
            .With(result => result.StudentId, query.StudentId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupStudentRepository.ExistsAsync(
                It.Is<CourseStudent>(parameter =>
                    parameter.CourseId == courseStudent.CourseId
                    && parameter.StudentId == courseStudent.StudentId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidateAsync_CourseNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetStudentCourseQuery>();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseRepository.ExistsAsync(query.CourseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyGroupStudentRepository.ExistsAsync(
                It.IsAny<CourseStudent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_CourseStudentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetStudentCourseQuery>();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseRepository.ExistsAsync(query.CourseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var courseStudent = _fixture.Build<CourseStudent>()
            .With(result => result.CourseId, query.CourseId)
            .With(result => result.StudentId, query.StudentId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyGroupStudentRepository.ExistsAsync(
                It.Is<CourseStudent>(parameter =>
                    parameter.CourseId == courseStudent.CourseId
                    && parameter.StudentId == courseStudent.StudentId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> action = () => _validator.ValidateAsync(query, CancellationToken.None);

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
