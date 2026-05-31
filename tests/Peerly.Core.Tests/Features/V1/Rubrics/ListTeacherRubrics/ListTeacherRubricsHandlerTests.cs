using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.ListTeacherRubrics;
using Peerly.Core.Models.Rubrics;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Rubrics.ListTeacherRubrics;

public sealed class ListTeacherRubricsHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();

    private readonly Fixture _fixture = new();
    private readonly ListTeacherRubricsHandler _handler;

    public ListTeacherRubricsHandlerTests()
    {
        SetupUnitOfWorkFactory();
        _handler = new ListTeacherRubricsHandler(_unitOfWorkFactoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_TeacherHasRubrics_ShouldReturnRubrics()
    {
        // Arrange
        var query = _fixture.Create<ListTeacherRubricsQuery>();

        var rubrics = new[] { _fixture.Create<Rubric>(), _fixture.Create<Rubric>() };
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyRubricRepository.ListByTeacherAsync(query.TeacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rubrics);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        response.Rubrics.Should().BeEquivalentTo(rubrics);
    }

    [Fact]
    public async Task ExecuteAsync_TeacherHasNoRubrics_ShouldReturnEmptyCollection()
    {
        // Arrange
        var query = _fixture.Create<ListTeacherRubricsQuery>();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyRubricRepository.ListByTeacherAsync(query.TeacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        response.Rubrics.Should().BeEmpty();
    }

    private void SetupUnitOfWorkFactory()
    {
        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);
    }
}
