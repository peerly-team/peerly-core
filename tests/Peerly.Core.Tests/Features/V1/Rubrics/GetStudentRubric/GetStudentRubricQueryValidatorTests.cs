using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.GetStudentRubric;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Rubrics;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Rubrics.GetStudentRubric;

public sealed class GetStudentRubricQueryValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();

    private readonly Fixture _fixture = new();
    private readonly GetStudentRubricQueryValidator _validator;

    public GetStudentRubricQueryValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new GetStudentRubricQueryValidator(unitOfWorkFactory);
    }

    [Fact]
    public async Task ValidateAsync_RubricExists_ShouldNotThrow()
    {
        // Arrange
        var query = _fixture.Create<GetStudentRubricQuery>();

        var rubric = _fixture.Build<Rubric>()
            .With(r => r.Id, query.RubricId)
            .Create();
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyRubricRepository.GetAsync(query.RubricId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rubric);

        // Act
        var act = async () => await _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidateAsync_RubricNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetStudentRubricQuery>();

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ReadOnlyRubricRepository.GetAsync(query.RubricId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Rubric?)null);

        // Act
        var act = async () => await _validator.ValidateAsync(query, CancellationToken.None);

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
