using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Courses.CreateCourse;

public sealed class CreateCourseHandlerTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IClock> _clockMock = new();
    private readonly Mock<ICommandValidator<CreateCourseCommand, CreateCourseCommandResponse>> _validatorMock = new();

    private readonly Fixture _fixture = new();
    private readonly CreateCourseHandler _handler;
    private readonly DateTimeOffset _currentTime;

    public CreateCourseHandlerTests()
    {
        var unitOfWorkFactoryMock = SetupUnitOfWorkFactory();

        _currentTime = _fixture.Create<DateTimeOffset>();
        _handler = new CreateCourseHandler(
            unitOfWorkFactoryMock,
            _clockMock.Object,
            _validatorMock.Object);
        _clockMock
            .Setup(clock => clock.GetCurrentTime())
            .Returns(_currentTime);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultSuccess_ShouldCreateCourse()
    {
        // Arrange
        var command = _fixture.Create<CreateCourseCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok);

        var courseId = _fixture.Create<CourseId>();
        var expectedCourseAddItem = _fixture.Build<CourseAddItem>()
            .With(result => result.Name, command.Name)
            .With(result => result.Description, command.Description)
            .With(result => result.Status, CourseStatus.Draft)
            .With(result => result.CreationTime, _currentTime)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CourseRepository.AddAsync(expectedCourseAddItem, It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseId);

        var expectedCourseTeacherAddItem = _fixture.Build<CourseTeacherAddItem>()
            .With(result => result.CourseId, courseId)
            .With(result => result.TeacherId, command.TeacherId)
            .With(result => result.CreationTime, _currentTime)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CourseTeacherRepository.AddAsync(expectedCourseTeacherAddItem, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fixture.Create<bool>());

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT0.Should().BeTrue();
        commandResponse.AsT0.CourseId.Should().BeEquivalentTo(courseId);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.CourseRepository.AddAsync(expectedCourseAddItem, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.CourseTeacherRepository.AddAsync(expectedCourseTeacherAddItem, It.IsAny<CancellationToken>()),
            Times.Once);
    }

        [Fact]
    public async Task ExecuteAsync_TeacherNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<CreateCourseCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtherError.NotFound(TeacherErrors.TeacherNotFound));

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT2.Should().BeTrue();
        commandResponse.AsT2.Type.Should().Be(ErrorType.NotFound);
        commandResponse.AsT2.Message.Should().Be(TeacherErrors.TeacherNotFound);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var unitOfWorkFactoryMock = new Mock<ICommonUnitOfWorkFactory>();
        unitOfWorkFactoryMock
            .Setup(factory => factory.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        var operationSetMock = new Mock<IOperationSet>();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.StartOperationSet(It.IsAny<CancellationToken>()))
            .ReturnsAsync(operationSetMock.Object);

        return unitOfWorkFactoryMock.Object;
    }
}
