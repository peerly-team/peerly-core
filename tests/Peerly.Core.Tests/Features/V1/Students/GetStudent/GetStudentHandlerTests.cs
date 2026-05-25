using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Students.GetStudent;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Students;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Students.GetStudent;

public sealed class GetStudentHandlerTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();

    private readonly Fixture _fixture = new();
    private readonly GetStudentHandler _handler;

    public GetStudentHandlerTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _handler = new GetStudentHandler(unitOfWorkFactory);
    }

    [Fact]
    public async Task ExecuteAsync_StudentExists_ShouldReturnStudent()
    {
        // Arrange
        var query = _fixture.Create<GetStudentQuery>();

        var student = _fixture.Build<Student>()
            .With(result => result.Id, query.StudentId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyStudentRepository.GetAsync(query.StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        // Act
        var result = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.Student.Should().Be(student);
    }

    [Fact]
    public async Task ExecuteAsync_StudentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetStudentQuery>();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyStudentRepository.GetAsync(query.StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

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
