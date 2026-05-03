using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourseFile;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Courses.CreateCourseFile;

public sealed class CreateCourseFileCommandValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();

    private readonly Fixture _fixture = new();
    private readonly CreateCourseFileCommandValidator _validator;

    public CreateCourseFileCommandValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new CreateCourseFileCommandValidator(unitOfWorkFactory);
    }

    [Fact]
    public async Task ValidateAsync_CourseTeacherExistsAndCourseExists_ShouldSuccess()
    {
        // Arrange
        var command = CreateCommand();

        var courseTeacher = _fixture.Build<CourseTeacher>()
            .With(result => result.CourseId, command.CourseId)
            .With(result => result.TeacherId, command.TeacherId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseRepository.ExistsAsync(command.CourseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_CourseTeacherNotFound_ShouldBeOtherErrorPermissionDenied()
    {
        // Arrange
        var command = CreateCommand();

        var courseTeacher = _fixture.Build<CourseTeacher>()
            .With(result => result.CourseId, command.CourseId)
            .With(result => result.TeacherId, command.TeacherId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.PermissionDenied);
        result.AsT2.Message.Should().BeNull();
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyCourseRepository.ExistsAsync(It.IsAny<CourseId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_CourseNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = CreateCommand();

        var courseTeacher = _fixture.Build<CourseTeacher>()
            .With(result => result.CourseId, command.CourseId)
            .With(result => result.TeacherId, command.TeacherId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyCourseRepository.ExistsAsync(command.CourseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(CourseErrors.CourseNotFound);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var unitOfWorkFactoryMock = new Mock<ICommonUnitOfWorkFactory>();
        unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        return unitOfWorkFactoryMock.Object;
    }

    private CreateCourseFileCommand CreateCommand()
    {
        return _fixture.Build<CreateCourseFileCommand>()
            .With(command => command.StorageId, (StorageId)Guid.NewGuid())
            .Create();
    }
}
