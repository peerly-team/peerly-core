using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Teachers.UpdateTeacher;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Teachers;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Teachers.UpdateTeacher;

public sealed class UpdateTeacherCommandValidatorTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();

    private readonly Fixture _fixture = new();
    private readonly UpdateTeacherCommandValidator _validator;

    public UpdateTeacherCommandValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new UpdateTeacherCommandValidator(unitOfWorkFactory);
    }

    [Fact]
    public async Task ValidateAsync_TeacherExists_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Create<UpdateTeacherCommand>();

        var teacher = _fixture.Build<Teacher>()
            .With(result => result.Id, command.TeacherId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.TeacherRepository.GetAsync(command.TeacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_TeacherNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<UpdateTeacherCommand>();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.TeacherRepository.GetAsync(command.TeacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Teacher?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
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
