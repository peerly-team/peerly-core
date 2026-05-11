using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetAssignedReview;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Submissions;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.GetAssignedReview;

public sealed class GetAssignedReviewHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlySubmittedHomeworkRepository> _submittedHomeworkRepositoryMock = new();
    private readonly Mock<IReadOnlyHomeworkRepository> _homeworkRepositoryMock = new();
    private readonly Mock<IReadOnlySubmittedHomeworkFileRepository> _submittedHomeworkFileRepositoryMock = new();
    private readonly Mock<IReadOnlySubmittedReviewRepository> _submittedReviewRepositoryMock = new();
    private readonly Mock<IQueryValidator<GetAssignedReviewQuery, GetAssignedReviewQueryResponse>> _validatorMock = new();
    private readonly Fixture _fixture = new();
    private readonly GetAssignedReviewHandler _handler;

    public GetAssignedReviewHandlerTests()
    {
        SetupUnitOfWorkFactory();
        _handler = new GetAssignedReviewHandler(_unitOfWorkFactoryMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationSuccess_ShouldReturnAssignedReview()
    {
        // Arrange
        var query = _fixture.Create<GetAssignedReviewQuery>();
        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(result => result.Id, query.SubmittedHomeworkId)
            .Create();
        var homework = _fixture.Build<Homework>()
            .With(result => result.Id, submittedHomework.HomeworkId)
            .Create();
        var files = new[]
        {
            _fixture.Build<File>().With(result => result.StorageId, (StorageId)Guid.NewGuid()).Create(),
            _fixture.Build<File>().With(result => result.StorageId, (StorageId)Guid.NewGuid()).Create()
        };
        var submittedReviewId = _fixture.Create<SubmittedReviewId>();
        var submittedHomeworkStudent = query.ToSubmittedHomeworkStudent();

        _submittedHomeworkRepositoryMock
            .Setup(repository => repository.GetAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomework);
        _homeworkRepositoryMock
            .Setup(repository => repository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);
        _submittedHomeworkFileRepositoryMock
            .Setup(repository => repository.ListAnonymizedBySubmittedHomeworkAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(files);
        _submittedReviewRepositoryMock
            .Setup(repository => repository.GetIdAsync(
                It.Is<SubmittedHomeworkStudent>(parameter => parameter == submittedHomeworkStudent),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedReviewId);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        response.SubmittedHomeworkId.Should().Be(submittedHomework.Id);
        response.Comment.Should().Be(submittedHomework.Comment);
        response.Checklist.Should().Be(homework.CheckList);
        response.Files.Should().BeEquivalentTo(files);
        response.SubmittedReviewId.Should().Be(submittedReviewId);
        _validatorMock.Verify(
            validator => validator.ValidateAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationFailed_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetAssignedReviewQuery>();
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
    }
}
