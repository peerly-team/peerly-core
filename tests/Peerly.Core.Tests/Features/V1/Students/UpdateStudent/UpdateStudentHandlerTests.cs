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
using Peerly.Core.ApplicationServices.Features.V1.Students.UpdateStudent;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Students;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Students.UpdateStudent;

public sealed class UpdateStudentHandlerTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICommandValidator<UpdateStudentCommand, Success>> _validatorMock = new();

    private readonly Fixture _fixture = new();
    private readonly UpdateStudentHandler _handler;

    public UpdateStudentHandlerTests()
    {
        var unitOfWorkFactoryMock = SetupUnitOfWorkFactory();

        _handler = new UpdateStudentHandler(
            unitOfWorkFactoryMock,
            _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultSuccess_ShouldUpdateStudent()
    {
        // Arrange
        var command = _fixture.Create<UpdateStudentCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok);

        var updateBuilderMock = new Mock<IUpdateBuilder<StudentUpdateItem>>();
        updateBuilderMock
            .Setup(builder => builder.Set(
                It.Is<Expression<Func<StudentUpdateItem, string>>>(expression => IsNameExpression(expression)),
                command.Name))
            .Returns(updateBuilderMock.Object);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.StudentRepository.UpdateAsync(
                command.StudentId,
                It.IsAny<Action<IUpdateBuilder<StudentUpdateItem>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<StudentId, Action<IUpdateBuilder<StudentUpdateItem>>, CancellationToken>(
                (_, configureUpdate, _) => configureUpdate(updateBuilderMock.Object))
            .ReturnsAsync(_fixture.Create<bool>());

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT0.Should().BeTrue();
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.StudentRepository.UpdateAsync(
                command.StudentId,
                It.IsAny<Action<IUpdateBuilder<StudentUpdateItem>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        updateBuilderMock.Verify(
            builder => builder.Set(
                It.Is<Expression<Func<StudentUpdateItem, string>>>(expression => IsNameExpression(expression)),
                command.Name),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultOtherError_ShouldBeOtherError()
    {
        // Arrange
        var command = _fixture.Create<UpdateStudentCommand>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtherError.NotFound());

        // Act
        var commandResponse = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        commandResponse.IsT2.Should().BeTrue();
        commandResponse.AsT2.Type.Should().Be(ErrorType.NotFound);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.StudentRepository.UpdateAsync(
                It.IsAny<StudentId>(),
                It.IsAny<Action<IUpdateBuilder<StudentUpdateItem>>>(),
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

    private static bool IsNameExpression(Expression<Func<StudentUpdateItem, string>> expression)
    {
        return expression.Body is MemberExpression { Member.Name: nameof(StudentUpdateItem.Name) };
    }
}
