using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetSubmittedReview;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.GetSubmittedReview;

public sealed class GetSubmittedReviewHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlySubmittedReviewRepository> _submittedReviewRepositoryMock = new();
    private readonly Mock<IReadOnlySubmittedReviewScoreRepository> _submittedReviewScoreRepositoryMock = new();
    private readonly Fixture _fixture = new();
    private readonly GetSubmittedReviewHandler _handler;

    public GetSubmittedReviewHandlerTests()
    {
        SetupUnitOfWorkFactory();
        _handler = new GetSubmittedReviewHandler(_unitOfWorkFactoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_SubmittedReviewBelongsToStudent_ShouldReturnSubmittedReview()
    {
        // Arrange
        var query = _fixture.Create<GetSubmittedReviewQuery>();
        var submittedReview = SetupSubmittedReview(query);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        response.SubmittedReview.Should().Be(submittedReview);
    }

    [Fact]
    public async Task ExecuteAsync_SubmittedReviewNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetSubmittedReviewQuery>();

        _submittedReviewRepositoryMock
            .Setup(repository => repository.GetAsync(query.SubmittedReviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubmittedReview?)null);

        // Act
        var action = () => _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_SubmittedReviewBelongsToAnotherStudent_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetSubmittedReviewQuery>();
        var otherStudentId = (StudentId)((long)query.StudentId + 1);
        var submittedReview = _fixture.Build<SubmittedReview>()
            .With(result => result.Id, query.SubmittedReviewId)
            .With(result => result.StudentId, otherStudentId)
            .Create();

        _submittedReviewRepositoryMock
            .Setup(repository => repository.GetAsync(query.SubmittedReviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedReview);

        // Act
        var action = () => _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    private SubmittedReview SetupSubmittedReview(GetSubmittedReviewQuery query)
    {
        var submittedReview = _fixture.Build<SubmittedReview>()
            .With(result => result.Id, query.SubmittedReviewId)
            .With(result => result.StudentId, query.StudentId)
            .With(result => result.Scores, [])
            .Create();
        _submittedReviewRepositoryMock
            .Setup(repository => repository.GetAsync(query.SubmittedReviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedReview);
        _submittedReviewScoreRepositoryMock
            .Setup(repository => repository.ListBySubmittedReviewIdAsync(query.SubmittedReviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        return submittedReview;
    }

    private void SetupUnitOfWorkFactory()
    {
        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySubmittedReviewRepository)
            .Returns(_submittedReviewRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySubmittedReviewScoreRepository)
            .Returns(_submittedReviewScoreRepositoryMock.Object);
    }
}
