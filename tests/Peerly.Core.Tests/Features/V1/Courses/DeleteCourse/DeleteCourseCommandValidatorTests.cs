using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Courses.DeleteCourse;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Courses.DeleteCourse;

public sealed class DeleteCourseCommandValidatorTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();

    private readonly Fixture _fixture = new();
    private readonly DeleteCourseCommandValidator _validator;

    public DeleteCourseCommandValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new DeleteCourseCommandValidator(unitOfWorkFactory);
    }

    [Fact]
    public async Task ValidateAsync_CourseTeacherExistsAndCourseInDraftStatus_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Create<DeleteCourseCommand>();

        var courseTeacher = _fixture.Build<CourseTeacher>()
            .With(result => result.CourseId, command.CourseId)
            .With(result => result.TeacherId, command.TeacherId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CourseTeacherRepository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var course = _fixture.Build<Course>()
            .With(result => result.Id, command.CourseId)
            .With(result => result.Status, CourseStatus.Draft)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CourseRepository.GetAsync(command.CourseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_CourseTeacherNotFound_ShouldBeOtherErrorPermissionDenied()
    {
        // Arrange
        var command = _fixture.Create<DeleteCourseCommand>();

        var courseTeacher = _fixture.Build<CourseTeacher>()
            .With(result => result.CourseId, command.CourseId)
            .With(result => result.TeacherId, command.TeacherId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CourseTeacherRepository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.PermissionDenied);
        result.AsT2.Message.Should().BeNull();
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.CourseRepository.GetAsync(It.IsAny<CourseId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_CourseNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<DeleteCourseCommand>();

        var courseTeacher = _fixture.Build<CourseTeacher>()
            .With(result => result.CourseId, command.CourseId)
            .With(result => result.TeacherId, command.TeacherId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CourseTeacherRepository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CourseRepository.GetAsync(command.CourseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Course?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(CourseErrors.CourseNotFound);
    }

    [Fact]
    public async Task ValidateAsync_CourseNotInDraftStatus_ShouldBeValidationError()
    {
        // Arrange
        var command = _fixture.Create<DeleteCourseCommand>();

        var courseTeacher = _fixture.Build<CourseTeacher>()
            .With(result => result.CourseId, command.CourseId)
            .With(result => result.TeacherId, command.TeacherId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CourseTeacherRepository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var course = _fixture.Build<Course>()
            .With(result => result.Id, command.CourseId)
            .With(result => result.Status, CourseStatus.InProgress)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CourseRepository.GetAsync(command.CourseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Errors.Should().NotBeNull().And.ContainSingle(CourseErrors.IncorrectCourseStatusForDelete);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var unitOfWorkFactoryMock = new Mock<ICommonUnitOfWorkFactory>();
        unitOfWorkFactoryMock
            .Setup(factory => factory.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        return unitOfWorkFactoryMock.Object;
    }
}
