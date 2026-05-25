using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Students.UpdateStudent;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Students;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Students.UpdateStudent;

public sealed class UpdateStudentCommandValidatorTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();

    private readonly Fixture _fixture = new();
    private readonly UpdateStudentCommandValidator _validator;

    public UpdateStudentCommandValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new UpdateStudentCommandValidator(unitOfWorkFactory);
    }

    [Fact]
    public async Task ValidateAsync_StudentExists_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Create<UpdateStudentCommand>();

        var student = _fixture.Build<Student>()
            .With(result => result.Id, command.StudentId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.StudentRepository.GetAsync(command.StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_StudentNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<UpdateStudentCommand>();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.StudentRepository.GetAsync(command.StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

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
