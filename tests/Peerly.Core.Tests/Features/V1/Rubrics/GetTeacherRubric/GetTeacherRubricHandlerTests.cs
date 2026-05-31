using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.GetTeacherRubric;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Rubrics;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Rubrics.GetTeacherRubric;

public sealed class GetTeacherRubricHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();

    private readonly Fixture _fixture = new();
    private readonly GetTeacherRubricHandler _handler;

    public GetTeacherRubricHandlerTests()
    {
        SetupUnitOfWorkFactory();
        _handler = new GetTeacherRubricHandler(_unitOfWorkFactoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_RubricExistsAndOwnedByTeacher_ShouldReturnRubricAndCriteria()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherRubricQuery>();

        var rubric = _fixture.Build<Rubric>()
            .With(r => r.Id, query.RubricId)
            .With(r => r.TeacherId, query.TeacherId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyRubricRepository.GetAsync(query.RubricId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rubric);

        var criteria = new[] { _fixture.Create<RubricCriterion>(), _fixture.Create<RubricCriterion>() };
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyRubricCriterionRepository.ListByRubricIdAsync(query.RubricId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(criteria);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        response.Rubric.Should().Be(rubric);
        response.Criteria.Should().BeEquivalentTo(criteria);
    }

    [Fact]
    public async Task ExecuteAsync_RubricNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherRubricQuery>();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyRubricRepository.GetAsync(query.RubricId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Rubric?)null);

        // Act
        var act = async () => await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.ReadOnlyRubricCriterionRepository.ListByRubricIdAsync(
                It.IsAny<RubricId>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_RubricBelongsToAnotherTeacher_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherRubricQuery>();

        var rubric = _fixture.Build<Rubric>()
            .With(r => r.Id, query.RubricId)
            .With(r => r.TeacherId, _fixture.Create<TeacherId>())
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyRubricRepository.GetAsync(query.RubricId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rubric);

        // Act
        var act = async () => await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    private void SetupUnitOfWorkFactory()
    {
        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);
    }
}
