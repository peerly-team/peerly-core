using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Participants.BulkAddGroupStudents;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Teachers;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Participants.BulkAddGroupStudents;

public sealed class BulkAddGroupStudentsValidatorTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlyGroupRepository> _groupRepositoryMock = new();
    private readonly Mock<IReadOnlyCourseTeacherRepository> _courseTeacherRepositoryMock = new();
    private readonly Mock<IReadOnlyTeacherRepository> _teacherRepositoryMock = new();

    private readonly Fixture _fixture = new();
    private readonly BulkAddGroupStudentsValidator _validator;

    public BulkAddGroupStudentsValidatorTests()
    {
        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyGroupRepository)
            .Returns(_groupRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyCourseTeacherRepository)
            .Returns(_courseTeacherRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyTeacherRepository)
            .Returns(_teacherRepositoryMock.Object);

        _validator = new BulkAddGroupStudentsValidator(_unitOfWorkFactoryMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_GroupNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<BulkAddGroupStudentsCommand>();
        _groupRepositoryMock
            .Setup(repository => repository.GetAsync(command.GroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        _courseTeacherRepositoryMock.Verify(
            repository => repository.ExistsAsync(It.IsAny<CourseTeacher>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_TeacherHasNoCourseAccess_ShouldBeOtherErrorPermissionDenied()
    {
        // Arrange
        var command = _fixture.Create<BulkAddGroupStudentsCommand>();
        var group = _fixture.Build<Group>()
            .With(item => item.Id, command.GroupId)
            .Create();
        _groupRepositoryMock
            .Setup(repository => repository.GetAsync(command.GroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var courseTeacher = new CourseTeacher
        {
            CourseId = group.CourseId,
            TeacherId = command.TeacherId
        };
        _courseTeacherRepositoryMock
            .Setup(repository => repository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.PermissionDenied);
        _teacherRepositoryMock.Verify(
            repository => repository.GetAsync(It.IsAny<TeacherId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_GroupExistsAndTeacherHasAccess_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Create<BulkAddGroupStudentsCommand>();
        var group = _fixture.Build<Group>()
            .With(item => item.Id, command.GroupId)
            .Create();
        _groupRepositoryMock
            .Setup(repository => repository.GetAsync(command.GroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var courseTeacher = new CourseTeacher
        {
            CourseId = group.CourseId,
            TeacherId = command.TeacherId
        };
        _courseTeacherRepositoryMock
            .Setup(repository => repository.ExistsAsync(courseTeacher, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _teacherRepositoryMock
            .Setup(repository => repository.GetAsync(command.TeacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fixture.Build<Teacher>().With(teacher => teacher.Id, command.TeacherId).Create());

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }
}
