using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Teachers;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Courses.CreateCourse;

public sealed class CreateCourseCommandValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();

    private readonly Fixture _fixture = new();
    private readonly CreateCourseCommandValidator _validator;

    public CreateCourseCommandValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new CreateCourseCommandValidator(unitOfWorkFactory);
    }

    [Fact]
    public async Task ValidateAsync_TeacherExists_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Create<CreateCourseCommand>();

        var teacher = _fixture.Build<Teacher>().With(result => result.Id, command.TeacherId).Create();
        var teacherFilter = _fixture.Build<TeacherFilter>().With(result => result.TeacherIds, [teacher.Id]).Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyTeacherRepository.ListAsync(
                It.Is<TeacherFilter>(parameter => !parameter.TeacherIds.Except(teacherFilter.TeacherIds).Any()),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([teacher]);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_TeacherNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<CreateCourseCommand>();

        var teacher = _fixture.Build<Teacher>().With(result => result.Id, command.TeacherId).Create();
        var teacherFilter = _fixture.Build<TeacherFilter>().With(result => result.TeacherIds, [teacher.Id]).Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyTeacherRepository.ListAsync(
                It.Is<TeacherFilter>(parameter => !parameter.TeacherIds.Except(teacherFilter.TeacherIds).Any()),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(TeacherErrors.TeacherNotFound);
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
