using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetAssignedReview;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Submissions;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.GetAssignedReview;

public sealed class GetAssignedReviewQueryValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlySubmittedHomeworkRepository> _submittedHomeworkRepositoryMock = new();
    private readonly Mock<IReadOnlyDistributionReviewerRepository> _distributionReviewerRepositoryMock = new();
    private readonly Mock<IReadOnlyHomeworkRepository> _homeworkRepositoryMock = new();
    private readonly Mock<IClock> _clockMock = new();
    private readonly Fixture _fixture = new();
    private readonly DateTimeOffset _currentTime = DateTimeOffset.UtcNow;
    private readonly GetAssignedReviewQueryValidator _validator;

    public GetAssignedReviewQueryValidatorTests()
    {
        _clockMock.Setup(clock => clock.GetCurrentTime()).Returns(_currentTime);
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new GetAssignedReviewQueryValidator(unitOfWorkFactory, _clockMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_AssignedReviewerAndHomeworkReviewingAndDeadlineInFuture_ShouldSuccess()
    {
        // Arrange
        var query = _fixture.Create<GetAssignedReviewQuery>();

        SetupAssignedReviewer(query, exists: true);

        var submittedHomework = SetupSubmittedHomework(query.SubmittedHomeworkId);
        SetupHomework(submittedHomework.HomeworkId, HomeworkStatus.Reviewing, _currentTime.AddDays(1));

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidateAsync_SubmittedHomeworkNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetAssignedReviewQuery>();

        SetupAssignedReviewer(query, exists: true);

        _submittedHomeworkRepositoryMock
            .Setup(repository => repository.GetAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubmittedHomework?)null);

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
        _homeworkRepositoryMock.Verify(
            repository => repository.GetAsync(It.IsAny<HomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_ReviewerIsNotAssigned_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetAssignedReviewQuery>();

        SetupAssignedReviewer(query, exists: false);

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
        _submittedHomeworkRepositoryMock.Verify(
            repository => repository.GetAsync(It.IsAny<SubmittedHomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _homeworkRepositoryMock.Verify(
            repository => repository.GetAsync(It.IsAny<HomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_LinkedHomeworkNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetAssignedReviewQuery>();

        SetupAssignedReviewer(query, exists: true);

        var submittedHomework = SetupSubmittedHomework(query.SubmittedHomeworkId);
        _homeworkRepositoryMock
            .Setup(repository => repository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Homework?)null);

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData(HomeworkStatus.Draft)]
    [InlineData(HomeworkStatus.Published)]
    [InlineData(HomeworkStatus.Confirmation)]
    [InlineData(HomeworkStatus.Finished)]
    public async Task ValidateAsync_HomeworkNotInReviewingStatus_ShouldThrowBusinessValidationException(HomeworkStatus status)
    {
        // Arrange
        var query = _fixture.Create<GetAssignedReviewQuery>();

        SetupAssignedReviewer(query, exists: true);

        var submittedHomework = SetupSubmittedHomework(query.SubmittedHomeworkId);
        SetupHomework(submittedHomework.HomeworkId, status, _currentTime.AddDays(1));

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<BusinessValidationException>();
        exception.Which.Message.Should().Be(HomeworkErrors.HomeworkNotAcceptingReviews);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ValidateAsync_ReviewDeadlineNotInFuture_ShouldThrowBusinessValidationException(int deadlineOffsetSeconds)
    {
        // Arrange
        var query = _fixture.Create<GetAssignedReviewQuery>();

        SetupAssignedReviewer(query, exists: true);

        var submittedHomework = SetupSubmittedHomework(query.SubmittedHomeworkId);
        SetupHomework(submittedHomework.HomeworkId, HomeworkStatus.Reviewing, _currentTime.AddSeconds(deadlineOffsetSeconds));

        // Act
        var action = () => _validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<BusinessValidationException>();
        exception.Which.Message.Should().Be(HomeworkErrors.HomeworkNotAcceptingReviews);
    }

    private SubmittedHomework SetupSubmittedHomework(SubmittedHomeworkId submittedHomeworkId)
    {
        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(result => result.Id, submittedHomeworkId)
            .Create();
        _submittedHomeworkRepositoryMock
            .Setup(repository => repository.GetAsync(submittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomework);

        return submittedHomework;
    }

    private void SetupAssignedReviewer(GetAssignedReviewQuery query, bool exists)
    {
        var submittedHomeworkStudent = query.ToSubmittedHomeworkStudent();
        _distributionReviewerRepositoryMock
            .Setup(repository => repository.ExistsAsync(
                It.Is<SubmittedHomeworkStudent>(parameter => parameter == submittedHomeworkStudent),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    private void SetupHomework(HomeworkId homeworkId, HomeworkStatus status, DateTimeOffset reviewDeadline)
    {
        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, homeworkId)
            .With(result => result.Status, status)
            .With(result => result.ReviewDeadline, reviewDeadline)
            .Create();
        _homeworkRepositoryMock
            .Setup(repository => repository.GetAsync(homeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var unitOfWorkFactoryMock = new Mock<ICommonUnitOfWorkFactory>();
        unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySubmittedHomeworkRepository)
            .Returns(_submittedHomeworkRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyDistributionReviewerRepository)
            .Returns(_distributionReviewerRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyHomeworkRepository)
            .Returns(_homeworkRepositoryMock.Object);

        return unitOfWorkFactoryMock.Object;
    }
}
