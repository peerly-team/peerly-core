using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.GetStudentRubric;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Rubrics;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Rubrics.GetStudentRubric;

public sealed class GetStudentRubricHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IQueryValidator<GetStudentRubricQuery, GetStudentRubricQueryResponse>> _validatorMock = new();

    private readonly Fixture _fixture = new();
    private readonly GetStudentRubricHandler _handler;

    public GetStudentRubricHandlerTests()
    {
        SetupUnitOfWorkFactory();
        _handler = new GetStudentRubricHandler(_unitOfWorkFactoryMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_RubricExists_ShouldReturnCriteria()
    {
        // Arrange
        var query = _fixture.Create<GetStudentRubricQuery>();

        var criteria = new[] { _fixture.Create<RubricCriterion>(), _fixture.Create<RubricCriterion>() };
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyRubricCriterionRepository.ListByRubricIdAsync(query.RubricId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(criteria);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        response.Criteria.Should().BeEquivalentTo(criteria);
        _validatorMock.Verify(
            validator => validator.ValidateAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatorThrows_ShouldPropagateNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetStudentRubricQuery>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

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
