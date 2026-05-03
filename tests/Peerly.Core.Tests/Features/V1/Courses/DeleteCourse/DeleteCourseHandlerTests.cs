using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OneOf.Types;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Courses.DeleteCourse;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Courses.DeleteCourse;

public sealed class DeleteCourseHandlerTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICommandValidator<DeleteCourseCommand, Success>> _validatorMock = new();

    private readonly Fixture _fixture = new();
    private readonly DeleteCourseHandler _handler;

    public DeleteCourseHandlerTests()
    {
        var unitOfWorkFactoryMock = SetupUnitOfWorkFactory();

        _handler = new DeleteCourseHandler(
            unitOfWorkFactoryMock,
            _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultSuccess_ShouldDeleteCourse()
    {
        // Arrange
        var command = _fixture.Create<DeleteCourseCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok);

        var updateBuilderMock = new Mock<IUpdateBuilder<CourseUpdateItem>>();
        updateBuilderMock
            .Setup(builder => builder.Set(
                It.Is<Expression<Func<CourseUpdateItem, CourseStatus>>>(expression => IsStatusExpression(expression)), CourseStatus.Deleted))
            .Returns(updateBuilderMock.Object);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CourseRepository.UpdateAsync(
                command.CourseId,
                It.IsAny<Action<IUpdateBuilder<CourseUpdateItem>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<CourseId, Action<IUpdateBuilder<CourseUpdateItem>>, CancellationToken>(
                (_, configureUpdate, _) => configureUpdate(updateBuilderMock.Object))
            .ReturnsAsync(_fixture.Create<bool>());

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT0.Should().BeTrue();
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.CourseRepository.UpdateAsync(
                command.CourseId,
                It.IsAny<Action<IUpdateBuilder<CourseUpdateItem>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        updateBuilderMock.Verify(
            builder => builder.Set(
                It.Is<Expression<Func<CourseUpdateItem, CourseStatus>>>(expression => IsStatusExpression(expression)),
                CourseStatus.Deleted),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultOtherError_ShouldBeOtherError()
    {
        // Arrange
        var command = _fixture.Create<DeleteCourseCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtherError.NotFound(CourseErrors.CourseNotFound));

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT2.Should().BeTrue();
        commandResponse.AsT2.Type.Should().Be(ErrorType.NotFound);
        commandResponse.AsT2.Message.Should().Be(CourseErrors.CourseNotFound);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.CourseRepository.UpdateAsync(
                It.IsAny<CourseId>(),
                It.IsAny<Action<IUpdateBuilder<CourseUpdateItem>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultValidationError_ShouldBeValidationError()
    {
        // Arrange
        var command = _fixture.Create<DeleteCourseCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationError.From(CourseErrors.IncorrectCourseStatusForDelete));

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT1.Should().BeTrue();
        commandResponse.AsT1.Errors.Should().NotBeNull().And.ContainSingle(CourseErrors.IncorrectCourseStatusForDelete.Value);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.CourseRepository.UpdateAsync(
                It.IsAny<CourseId>(),
                It.IsAny<Action<IUpdateBuilder<CourseUpdateItem>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var unitOfWorkFactoryMock = new Mock<ICommonUnitOfWorkFactory>();
        unitOfWorkFactoryMock
            .Setup(factory => factory.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        return unitOfWorkFactoryMock.Object;
    }

    private static bool IsStatusExpression(Expression<Func<CourseUpdateItem, CourseStatus>> expression)
    {
        return expression.Body is MemberExpression { Member.Name: nameof(CourseUpdateItem.Status) };
    }
}
