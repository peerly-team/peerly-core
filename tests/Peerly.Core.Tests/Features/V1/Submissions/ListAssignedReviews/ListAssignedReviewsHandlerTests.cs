using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.ListAssignedReviews;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.ListAssignedReviews;

public sealed class ListAssignedReviewsHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlyDistributionReviewerRepository> _distributionReviewerRepositoryMock = new();
    private readonly Mock<IReadOnlySubmittedReviewRepository> _submittedReviewRepositoryMock = new();
    private readonly Mock<IReadOnlyHomeworkRepository> _homeworkRepositoryMock = new();
    private readonly Mock<IQueryValidator<ListAssignedReviewsQuery, ListAssignedReviewsQueryResponse>> _validatorMock = new();
    private readonly Fixture _fixture = new();
    private readonly ListAssignedReviewsHandler _handler;

    public ListAssignedReviewsHandlerTests()
    {
        SetupUnitOfWorkFactory();
        _handler = new ListAssignedReviewsHandler(_unitOfWorkFactoryMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationSuccess_ShouldReturnAssignedReviews()
    {
        // Arrange
        var query = _fixture.Create<ListAssignedReviewsQuery>();
        var homework = SetupHomework(query.HomeworkId);
        var assignedIds = new[]
        {
            _fixture.Create<SubmittedHomeworkId>(),
            _fixture.Create<SubmittedHomeworkId>()
        };
        var reviewedIds = new[]
        {
            assignedIds[1],
            _fixture.Create<SubmittedHomeworkId>()
        };

        SetupValidatorSuccess(query);
        SetupAssignedReviews(query, assignedIds);
        SetupReviewedReviews(query, reviewedIds);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        response.AssignedReviews.Should().BeEquivalentTo(
            [
                new
                {
                    SubmittedHomeworkId = assignedIds[0],
                    HomeworkName = homework.Name,
                    IsReviewed = false
                },
                new
                {
                    SubmittedHomeworkId = assignedIds[1],
                    HomeworkName = homework.Name,
                    IsReviewed = true
                }
            ],
            options => options.WithStrictOrdering());
        _validatorMock.Verify(
            validator => validator.ValidateAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_AssignedReviewsEmpty_ShouldReturnEmptyCollection()
    {
        // Arrange
        var query = _fixture.Create<ListAssignedReviewsQuery>();
        SetupHomework(query.HomeworkId);
        SetupValidatorSuccess(query);
        SetupAssignedReviews(query, []);
        SetupReviewedReviews(query, [_fixture.Create<SubmittedHomeworkId>()]);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        response.AssignedReviews.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ValidationFailed_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<ListAssignedReviewsQuery>();
        _validatorMock
            .Setup(validator => validator.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

        // Act
        var action = () => _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkFactoryMock.Verify(
            factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupUnitOfWorkFactory()
    {
        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyDistributionReviewerRepository)
            .Returns(_distributionReviewerRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySubmittedReviewRepository)
            .Returns(_submittedReviewRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyHomeworkRepository)
            .Returns(_homeworkRepositoryMock.Object);
    }

    private void SetupValidatorSuccess(ListAssignedReviewsQuery query)
    {
        _validatorMock
            .Setup(validator => validator.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private Homework SetupHomework(HomeworkId homeworkId)
    {
        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, homeworkId)
            .Create();
        _homeworkRepositoryMock
            .Setup(repository => repository.GetAsync(homeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        return homework;
    }

    private void SetupAssignedReviews(ListAssignedReviewsQuery query, SubmittedHomeworkId[] assignedIds)
    {
        var homeworkStudent = query.ToHomeworkStudent();
        _distributionReviewerRepositoryMock
            .Setup(repository => repository.ListAssignedByAsync(homeworkStudent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignedIds);
    }

    private void SetupReviewedReviews(ListAssignedReviewsQuery query, SubmittedHomeworkId[] reviewedIds)
    {
        var homeworkStudent = query.ToHomeworkStudent();
        _submittedReviewRepositoryMock
            .Setup(repository => repository.ListReviewedByAsync(homeworkStudent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviewedIds);
    }
}
