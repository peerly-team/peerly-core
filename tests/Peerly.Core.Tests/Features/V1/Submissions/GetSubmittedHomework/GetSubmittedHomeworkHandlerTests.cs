using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetSubmittedHomework;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Submissions;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.GetSubmittedHomework;

public sealed class GetSubmittedHomeworkHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlySubmittedHomeworkRepository> _submittedHomeworkRepositoryMock = new();
    private readonly Mock<IReadOnlyHomeworkRepository> _homeworkRepositoryMock = new();
    private readonly Mock<IReadOnlySubmittedHomeworkFileRepository> _submittedHomeworkFileRepositoryMock = new();
    private readonly Mock<IReadOnlySubmittedReviewRepository> _submittedReviewRepositoryMock = new();
    private readonly Mock<IReadOnlySubmittedHomeworkMarkRepository> _submittedHomeworkMarkRepositoryMock = new();
    private readonly Mock<IReadOnlySubmittedReviewScoreRepository> _submittedReviewScoreRepositoryMock = new();
    private readonly Fixture _fixture = new();
    private readonly GetSubmittedHomeworkHandler _handler;

    public GetSubmittedHomeworkHandlerTests()
    {
        SetupUnitOfWorkFactory();
        _handler = new GetSubmittedHomeworkHandler(_unitOfWorkFactoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_SubmittedHomeworkBelongsToStudent_ShouldReturnSubmittedHomeworkFilesReviewsAndFinalMark()
    {
        // Arrange
        var query = _fixture.Create<GetSubmittedHomeworkQuery>();
        var submittedHomework = SetupSubmittedHomework(query);
        SetupHomework(submittedHomework.HomeworkId, HomeworkStatus.Finished);
        var files = new[]
        {
            _fixture.Build<File>().With(result => result.StorageId, (StorageId)Guid.NewGuid()).Create(),
            _fixture.Build<File>().With(result => result.StorageId, (StorageId)Guid.NewGuid()).Create()
        };
        var reviews = new[]
        {
            _fixture.Build<SubmittedReview>().With(result => result.SubmittedHomeworkId, query.SubmittedHomeworkId).With(result => result.Scores, []).Create(),
            _fixture.Build<SubmittedReview>().With(result => result.SubmittedHomeworkId, query.SubmittedHomeworkId).With(result => result.Scores, []).Create()
        };
        var submittedHomeworkMark = _fixture.Build<SubmittedHomeworkMark>()
            .With(result => result.SubmittedHomeworkId, query.SubmittedHomeworkId)
            .With(result => result.ReviewersMark, 82)
            .With(result => result.TeacherMark, (int?)null)
            .Create();

        _submittedHomeworkFileRepositoryMock
            .Setup(repository => repository.ListBySubmittedHomeworkAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(files);
        _submittedReviewRepositoryMock
            .Setup(repository => repository.ListBySubmittedHomeworkAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviews);
        _submittedHomeworkMarkRepositoryMock
            .Setup(repository => repository.GetBySubmittedHomeworkAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomeworkMark);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        response.SubmittedHomework.Should().Be(submittedHomework);
        response.Files.Should().BeEquivalentTo(files);
        response.SubmittedReviews.Should().BeEquivalentTo(reviews);
        response.FinalMark.Should().Be(submittedHomeworkMark.ReviewersMark);
    }

    [Fact]
    public async Task ExecuteAsync_TeacherMarkExists_ShouldUseTeacherMarkAsFinalMark()
    {
        // Arrange
        var query = _fixture.Create<GetSubmittedHomeworkQuery>();
        var submittedHomework = SetupSubmittedHomework(query);
        SetupHomework(submittedHomework.HomeworkId, HomeworkStatus.Finished);
        var submittedHomeworkMark = _fixture.Build<SubmittedHomeworkMark>()
            .With(result => result.SubmittedHomeworkId, query.SubmittedHomeworkId)
            .With(result => result.ReviewersMark, 70)
            .With(result => result.TeacherMark, 95)
            .Create();

        _submittedHomeworkFileRepositoryMock
            .Setup(repository => repository.ListBySubmittedHomeworkAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _submittedReviewRepositoryMock
            .Setup(repository => repository.ListBySubmittedHomeworkAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _submittedHomeworkMarkRepositoryMock
            .Setup(repository => repository.GetBySubmittedHomeworkAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomeworkMark);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        response.FinalMark.Should().Be(submittedHomeworkMark.TeacherMark);
    }

    [Fact]
    public async Task ExecuteAsync_MarkNotFound_ShouldReturnNullFinalMark()
    {
        // Arrange
        var query = _fixture.Create<GetSubmittedHomeworkQuery>();
        var submittedHomework = SetupSubmittedHomework(query);
        SetupHomework(submittedHomework.HomeworkId, HomeworkStatus.Finished);

        _submittedHomeworkFileRepositoryMock
            .Setup(repository => repository.ListBySubmittedHomeworkAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _submittedReviewRepositoryMock
            .Setup(repository => repository.ListBySubmittedHomeworkAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _submittedHomeworkMarkRepositoryMock
            .Setup(repository => repository.GetBySubmittedHomeworkAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubmittedHomeworkMark?)null);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        response.FinalMark.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_HomeworkNotFinished_ShouldHideReviewsAndFinalMark()
    {
        // Arrange
        var query = _fixture.Create<GetSubmittedHomeworkQuery>();
        var submittedHomework = SetupSubmittedHomework(query);
        SetupHomework(submittedHomework.HomeworkId, HomeworkStatus.Confirmation);
        var files = new[]
        {
            _fixture.Build<File>().With(result => result.StorageId, (StorageId)Guid.NewGuid()).Create()
        };

        _submittedHomeworkFileRepositoryMock
            .Setup(repository => repository.ListBySubmittedHomeworkAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(files);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        response.SubmittedHomework.Should().Be(submittedHomework);
        response.Files.Should().BeEquivalentTo(files);
        response.SubmittedReviews.Should().BeEmpty();
        response.FinalMark.Should().BeNull();
        _submittedReviewRepositoryMock.Verify(
            repository => repository.ListBySubmittedHomeworkAsync(It.IsAny<SubmittedHomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _submittedHomeworkMarkRepositoryMock.Verify(
            repository => repository.GetBySubmittedHomeworkAsync(It.IsAny<SubmittedHomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_LinkedHomeworkNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetSubmittedHomeworkQuery>();
        var submittedHomework = SetupSubmittedHomework(query);

        _homeworkRepositoryMock
            .Setup(repository => repository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Homework?)null);

        // Act
        var action = () => _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
        _submittedReviewRepositoryMock.Verify(
            repository => repository.ListBySubmittedHomeworkAsync(It.IsAny<SubmittedHomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _submittedHomeworkFileRepositoryMock.Verify(
            repository => repository.ListBySubmittedHomeworkAsync(It.IsAny<SubmittedHomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _submittedHomeworkMarkRepositoryMock.Verify(
            repository => repository.GetBySubmittedHomeworkAsync(It.IsAny<SubmittedHomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_SubmittedHomeworkNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetSubmittedHomeworkQuery>();

        _submittedHomeworkRepositoryMock
            .Setup(repository => repository.GetAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubmittedHomework?)null);

        // Act
        var action = () => _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
        _submittedHomeworkFileRepositoryMock.Verify(
            repository => repository.ListBySubmittedHomeworkAsync(It.IsAny<SubmittedHomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _submittedReviewRepositoryMock.Verify(
            repository => repository.ListBySubmittedHomeworkAsync(It.IsAny<SubmittedHomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _submittedHomeworkMarkRepositoryMock.Verify(
            repository => repository.GetBySubmittedHomeworkAsync(It.IsAny<SubmittedHomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_SubmittedHomeworkBelongsToAnotherStudent_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetSubmittedHomeworkQuery>();
        var otherStudentId = (StudentId)((long)query.StudentId + 1);
        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(result => result.Id, query.SubmittedHomeworkId)
            .With(result => result.StudentId, otherStudentId)
            .Create();

        _submittedHomeworkRepositoryMock
            .Setup(repository => repository.GetAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomework);

        // Act
        var action = () => _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
        _submittedHomeworkFileRepositoryMock.Verify(
            repository => repository.ListBySubmittedHomeworkAsync(It.IsAny<SubmittedHomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _submittedReviewRepositoryMock.Verify(
            repository => repository.ListBySubmittedHomeworkAsync(It.IsAny<SubmittedHomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _submittedHomeworkMarkRepositoryMock.Verify(
            repository => repository.GetBySubmittedHomeworkAsync(It.IsAny<SubmittedHomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private SubmittedHomework SetupSubmittedHomework(GetSubmittedHomeworkQuery query)
    {
        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(result => result.Id, query.SubmittedHomeworkId)
            .With(result => result.StudentId, query.StudentId)
            .Create();
        _submittedHomeworkRepositoryMock
            .Setup(repository => repository.GetAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomework);

        return submittedHomework;
    }

    private void SetupHomework(HomeworkId homeworkId, HomeworkStatus status)
    {
        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, homeworkId)
            .With(result => result.Status, status)
            .Create();
        _homeworkRepositoryMock
            .Setup(repository => repository.GetAsync(homeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);
    }

    private void SetupUnitOfWorkFactory()
    {
        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySubmittedHomeworkRepository)
            .Returns(_submittedHomeworkRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyHomeworkRepository)
            .Returns(_homeworkRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySubmittedHomeworkFileRepository)
            .Returns(_submittedHomeworkFileRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySubmittedReviewRepository)
            .Returns(_submittedReviewRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySubmittedHomeworkMarkRepository)
            .Returns(_submittedHomeworkMarkRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySubmittedReviewScoreRepository)
            .Returns(_submittedReviewScoreRepositoryMock.Object);

        _submittedReviewScoreRepositoryMock
            .Setup(repository => repository.ListBySubmittedReviewIdsAsync(It.IsAny<IReadOnlyCollection<SubmittedReviewId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
    }
}
