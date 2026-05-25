using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Teachers.GetTeacher;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Teachers;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Teachers.GetTeacher;

public sealed class GetTeacherHandlerTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();

    private readonly Fixture _fixture = new();
    private readonly GetTeacherHandler _handler;

    public GetTeacherHandlerTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _handler = new GetTeacherHandler(unitOfWorkFactory);
    }

    [Fact]
    public async Task ExecuteAsync_TeacherExists_ShouldReturnTeacher()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherQuery>();

        var teacher = _fixture.Build<Teacher>()
            .With(result => result.Id, query.TeacherId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyTeacherRepository.GetAsync(query.TeacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        // Act
        var result = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.Teacher.Should().Be(teacher);
    }

    [Fact]
    public async Task ExecuteAsync_TeacherNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherQuery>();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyTeacherRepository.GetAsync(query.TeacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Teacher?)null);

        // Act
        var act = async () => await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
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
