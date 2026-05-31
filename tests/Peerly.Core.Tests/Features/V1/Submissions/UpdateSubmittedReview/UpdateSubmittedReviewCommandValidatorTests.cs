using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.Shared.Models;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.UpdateSubmittedReview;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Rubrics;
using Peerly.Core.Models.Submissions;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.UpdateSubmittedReview;

public sealed class UpdateSubmittedReviewCommandValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlySubmittedReviewRepository> _submittedReviewRepositoryMock = new();
    private readonly Mock<IReadOnlySubmittedHomeworkRepository> _submittedHomeworkRepositoryMock = new();
    private readonly Mock<IReadOnlyHomeworkRepository> _homeworkRepositoryMock = new();
    private readonly Mock<IReadOnlyRubricCriterionRepository> _rubricCriterionRepositoryMock = new();
    private readonly Mock<IClock> _clockMock = new();
    private readonly Fixture _fixture = new();
    private readonly DateTimeOffset _currentTime = DateTimeOffset.UtcNow;
    private readonly UpdateSubmittedReviewCommandValidator _validator;

    public UpdateSubmittedReviewCommandValidatorTests()
    {
        _clockMock.Setup(clock => clock.GetCurrentTime()).Returns(_currentTime);
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new UpdateSubmittedReviewCommandValidator(unitOfWorkFactory, _clockMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_ReviewBelongsToStudentAndHomeworkReviewingAndDeadlineInFuture_ShouldSuccess()
    {
        // Arrange
        var rubricId = _fixture.Create<RubricId>();
        var criterionId = _fixture.Create<RubricCriterionId>();

        var criterion = _fixture.Build<RubricCriterion>()
            .With(c => c.Id, criterionId)
            .With(c => c.RubricId, rubricId)
            .With(c => c.MaxScore, 10)
            .With(c => c.CommentRequired, false)
            .Create();

        IReadOnlyCollection<SubmittedReviewScoreItem> scores = new List<SubmittedReviewScoreItem>
        {
            new() { RubricCriterionId = criterionId, Score = 5 },
        };

        var command = _fixture.Build<UpdateSubmittedReviewCommand>()
            .With(c => c.Scores, scores)
            .Create();

        var submittedReview = SetupSubmittedReview(command);
        var submittedHomework = SetupSubmittedHomework(submittedReview.SubmittedHomeworkId);
        SetupHomework(submittedHomework.HomeworkId, HomeworkStatus.Reviewing, _currentTime.AddDays(1), rubricId);

        _rubricCriterionRepositoryMock
            .Setup(repository => repository.ListByRubricIdAsync(rubricId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([criterion]);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_SubmittedReviewNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<UpdateSubmittedReviewCommand>();
        _submittedReviewRepositoryMock
            .Setup(repository => repository.GetAsync(command.SubmittedReviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubmittedReview?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(SubmittedReviewErrors.SubmittedReviewNotFound);
        _submittedHomeworkRepositoryMock.Verify(
            repository => repository.GetAsync(It.IsAny<SubmittedHomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_SubmittedReviewBelongsToAnotherStudent_ShouldBeOtherErrorPermissionDenied()
    {
        // Arrange
        var command = _fixture.Create<UpdateSubmittedReviewCommand>();
        var submittedReview = _fixture.Build<SubmittedReview>()
            .With(result => result.Id, command.SubmittedReviewId)
            .With(result => result.StudentId, (StudentId)((long)command.StudentId + 1))
            .Create();
        _submittedReviewRepositoryMock
            .Setup(repository => repository.GetAsync(command.SubmittedReviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedReview);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.PermissionDenied);
        _submittedHomeworkRepositoryMock.Verify(
            repository => repository.GetAsync(It.IsAny<SubmittedHomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _homeworkRepositoryMock.Verify(
            repository => repository.GetAsync(It.IsAny<HomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_SubmittedHomeworkNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<UpdateSubmittedReviewCommand>();
        var submittedReview = SetupSubmittedReview(command);
        _submittedHomeworkRepositoryMock
            .Setup(repository => repository.GetAsync(submittedReview.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubmittedHomework?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(SubmittedHomeworkErrors.SubmittedHomeworkNotFound);
        _homeworkRepositoryMock.Verify(
            repository => repository.GetAsync(It.IsAny<HomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_HomeworkNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<UpdateSubmittedReviewCommand>();
        var submittedReview = SetupSubmittedReview(command);
        var submittedHomework = SetupSubmittedHomework(submittedReview.SubmittedHomeworkId);
        _homeworkRepositoryMock
            .Setup(repository => repository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Homework?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(HomeworkErrors.HomeworkNotFound);
    }

    [Theory]
    [InlineData(HomeworkStatus.Draft)]
    [InlineData(HomeworkStatus.Published)]
    [InlineData(HomeworkStatus.Confirmation)]
    [InlineData(HomeworkStatus.Finished)]
    public async Task ValidateAsync_HomeworkNotInReviewingStatus_ShouldBeValidationError(HomeworkStatus status)
    {
        // Arrange
        var command = _fixture.Create<UpdateSubmittedReviewCommand>();
        var submittedReview = SetupSubmittedReview(command);
        var submittedHomework = SetupSubmittedHomework(submittedReview.SubmittedHomeworkId);
        SetupHomework(submittedHomework.HomeworkId, status, _currentTime.AddDays(1));

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Errors.Should().NotBeNull().And.ContainSingle(HomeworkErrors.HomeworkNotAcceptingReviews);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ValidateAsync_ReviewDeadlineNotInFuture_ShouldBeValidationError(int deadlineOffsetSeconds)
    {
        // Arrange
        var command = _fixture.Create<UpdateSubmittedReviewCommand>();
        var submittedReview = SetupSubmittedReview(command);
        var submittedHomework = SetupSubmittedHomework(submittedReview.SubmittedHomeworkId);
        SetupHomework(submittedHomework.HomeworkId, HomeworkStatus.Reviewing, _currentTime.AddSeconds(deadlineOffsetSeconds));

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Errors.Should().NotBeNull().And.ContainSingle(HomeworkErrors.HomeworkNotAcceptingReviews);
    }

    private SubmittedReview SetupSubmittedReview(UpdateSubmittedReviewCommand command)
    {
        var submittedReview = _fixture.Build<SubmittedReview>()
            .With(result => result.Id, command.SubmittedReviewId)
            .With(result => result.StudentId, command.StudentId)
            .Create();
        _submittedReviewRepositoryMock
            .Setup(repository => repository.GetAsync(command.SubmittedReviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedReview);

        return submittedReview;
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

    private void SetupHomework(HomeworkId homeworkId, HomeworkStatus status, DateTimeOffset reviewDeadline, RubricId? rubricId = null)
    {
        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, homeworkId)
            .With(result => result.Status, status)
            .With(result => result.ReviewDeadline, reviewDeadline)
            .With(result => result.RubricId, rubricId)
            .Create();
        _homeworkRepositoryMock
            .Setup(repository => repository.GetAsync(homeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var factoryMock = new Mock<ICommonUnitOfWorkFactory>();
        factoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySubmittedReviewRepository)
            .Returns(_submittedReviewRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySubmittedHomeworkRepository)
            .Returns(_submittedHomeworkRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyHomeworkRepository)
            .Returns(_homeworkRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyRubricCriterionRepository)
            .Returns(_rubricCriterionRepositoryMock.Object);

        return factoryMock.Object;
    }
}
