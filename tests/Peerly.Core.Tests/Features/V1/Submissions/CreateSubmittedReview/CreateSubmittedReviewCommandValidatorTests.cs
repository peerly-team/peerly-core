using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Submissions;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.CreateSubmittedReview;

public sealed class CreateSubmittedReviewCommandValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlySubmittedHomeworkRepository> _submittedHomeworkRepositoryMock = new();
    private readonly Mock<IReadOnlyDistributionReviewerRepository> _distributionReviewerRepositoryMock = new();
    private readonly Mock<IReadOnlyHomeworkRepository> _homeworkRepositoryMock = new();
    private readonly Mock<IReadOnlySubmittedReviewRepository> _submittedReviewRepositoryMock = new();
    private readonly Mock<IClock> _clockMock = new();
    private readonly Fixture _fixture = new();
    private readonly DateTimeOffset _currentTime = DateTimeOffset.UtcNow;
    private readonly CreateSubmittedReviewCommandValidator _validator;

    public CreateSubmittedReviewCommandValidatorTests()
    {
        _clockMock.Setup(clock => clock.GetCurrentTime()).Returns(_currentTime);
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new CreateSubmittedReviewCommandValidator(unitOfWorkFactory, _clockMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_AssignedReviewerAndHomeworkReviewingAndDeadlineInFuture_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedReviewCommand>();

        var submittedHomework = SetupSubmittedHomework(command.SubmittedHomeworkId);
        SetupHomework(submittedHomework.HomeworkId, HomeworkStatus.Reviewing, _currentTime.AddDays(1));

        var submittedHomeworkStudent = SetupAssignedReviewer(command, exists: true);
        SetupSubmittedReviewExists(submittedHomeworkStudent, exists: false);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_SubmittedHomeworkNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedReviewCommand>();

        _submittedHomeworkRepositoryMock
            .Setup(repository => repository.GetAsync(command.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubmittedHomework?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(SubmittedHomeworkErrors.SubmittedHomeworkNotFound);
        _distributionReviewerRepositoryMock.Verify(
            repository => repository.ExistsAsync(It.IsAny<SubmittedHomeworkStudent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_ReviewerIsNotAssigned_ShouldBeOtherErrorPermissionDenied()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedReviewCommand>();

        SetupSubmittedHomework(command.SubmittedHomeworkId);
        SetupAssignedReviewer(command, exists: false);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.PermissionDenied);
        _homeworkRepositoryMock.Verify(
            repository => repository.GetAsync(It.IsAny<HomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_LinkedHomeworkNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedReviewCommand>();

        SetupAssignedReviewer(command, exists: true);

        var submittedHomework = SetupSubmittedHomework(command.SubmittedHomeworkId);
        _homeworkRepositoryMock
            .Setup(repository => repository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Homework?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
        result.AsT2.Message.Should().Be(HomeworkErrors.HomeworkNotFound);
        _submittedReviewRepositoryMock.Verify(
            repository => repository.ExistsAsync(It.IsAny<SubmittedHomeworkStudent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(HomeworkStatus.Draft)]
    [InlineData(HomeworkStatus.Published)]
    [InlineData(HomeworkStatus.Confirmation)]
    [InlineData(HomeworkStatus.Finished)]
    public async Task ValidateAsync_HomeworkNotInReviewingStatus_ShouldBeValidationError(HomeworkStatus status)
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedReviewCommand>();

        var submittedHomework = SetupSubmittedHomework(command.SubmittedHomeworkId);
        SetupHomework(submittedHomework.HomeworkId, status, _currentTime.AddDays(1));

        SetupAssignedReviewer(command, exists: true);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Errors.Should().NotBeNull().And.ContainSingle(HomeworkErrors.HomeworkNotAcceptingReviews);
        _submittedReviewRepositoryMock.Verify(
            repository => repository.ExistsAsync(It.IsAny<SubmittedHomeworkStudent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ValidateAsync_ReviewDeadlineNotInFuture_ShouldBeValidationError(int deadlineOffsetSeconds)
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedReviewCommand>();

        var submittedHomework = SetupSubmittedHomework(command.SubmittedHomeworkId);
        SetupHomework(submittedHomework.HomeworkId, HomeworkStatus.Reviewing, _currentTime.AddSeconds(deadlineOffsetSeconds));

        SetupAssignedReviewer(command, exists: true);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Errors.Should().NotBeNull().And.ContainSingle(HomeworkErrors.HomeworkNotAcceptingReviews);
        _submittedReviewRepositoryMock.Verify(
            repository => repository.ExistsAsync(It.IsAny<SubmittedHomeworkStudent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_SubmittedReviewAlreadyExists_ShouldBeOtherErrorConflict()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedReviewCommand>();

        var submittedHomework = SetupSubmittedHomework(command.SubmittedHomeworkId);
        SetupHomework(submittedHomework.HomeworkId, HomeworkStatus.Reviewing, _currentTime.AddDays(1));

        var submittedHomeworkStudent = SetupAssignedReviewer(command, exists: true);
        SetupSubmittedReviewExists(submittedHomeworkStudent, exists: true);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.Conflict);
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

    private SubmittedHomeworkStudent SetupAssignedReviewer(CreateSubmittedReviewCommand command, bool exists)
    {
        var submittedHomeworkStudent = new SubmittedHomeworkStudent
        {
            SubmittedHomeworkId = command.SubmittedHomeworkId,
            StudentId = command.StudentId
        };
        _distributionReviewerRepositoryMock
            .Setup(repository => repository.ExistsAsync(submittedHomeworkStudent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);

        return submittedHomeworkStudent;
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

    private void SetupSubmittedReviewExists(SubmittedHomeworkStudent submittedHomeworkStudent, bool exists)
    {
        _submittedReviewRepositoryMock
            .Setup(repository => repository.ExistsAsync(submittedHomeworkStudent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var factoryMock = new Mock<ICommonUnitOfWorkFactory>();
        factoryMock
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
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySubmittedReviewRepository)
            .Returns(_submittedReviewRepositoryMock.Object);

        return factoryMock.Object;
    }
}
