using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetStudentHomework;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Homeworks;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Homeworks.GetStudentHomework;

public sealed class GetStudentHomeworkHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlyHomeworkRepository> _homeworkRepositoryMock = new();
    private readonly Mock<IReadOnlySubmittedHomeworkRepository> _submittedHomeworkRepositoryMock = new();
    private readonly Mock<IReadOnlyHomeworkFileRepository> _homeworkFileRepositoryMock = new();
    private readonly Mock<IQueryValidator<GetStudentHomeworkQuery, GetStudentHomeworkQueryResponse>> _validatorMock = new();

    private readonly Fixture _fixture = new();
    private readonly GetStudentHomeworkHandler _handler;

    public GetStudentHomeworkHandlerTests()
    {
        SetupUnitOfWorkFactory();
        _handler = new GetStudentHomeworkHandler(_unitOfWorkFactoryMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationSuccess_ShouldReturnStudentHomework()
    {
        // Arrange
        var query = _fixture.Create<GetStudentHomeworkQuery>();
        var studentHomework = _fixture.Build<StudentHomeworkInfo>()
            .With(result => result.Id, query.HomeworkId)
            .Create();
        var submittedHomeworkId = _fixture.Create<SubmittedHomeworkId>();
        var files = new[]
        {
            _fixture.Build<File>().With(result => result.StorageId, (StorageId)Guid.NewGuid()).Create(),
            _fixture.Build<File>().With(result => result.StorageId, (StorageId)Guid.NewGuid()).Create()
        };

        SetupValidatorSuccess(query);
        SetupGetStudentHomework(query, studentHomework);
        SetupGetSubmittedHomeworkId(query, submittedHomeworkId);
        _homeworkFileRepositoryMock
            .Setup(repository => repository.ListFilesAsync(query.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(files);

        // Act
        var queryResponse = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        queryResponse.StudentHomeworkInfo.Should().BeEquivalentTo(studentHomework);
        queryResponse.SubmittedHomeworkId.Should().Be(submittedHomeworkId);
        queryResponse.Files.Should().BeEquivalentTo(files);
        _validatorMock.Verify(
            validator => validator.ValidateAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
        _homeworkRepositoryMock.Verify(
            repository => repository.GetStudentHomeworkInfoAsync(
                It.Is<HomeworkStudent>(parameter =>
                    parameter.HomeworkId == query.HomeworkId
                    && parameter.StudentId == query.StudentId),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _submittedHomeworkRepositoryMock.Verify(
            repository => repository.GetSubmittedHomeworkIdAsync(
                It.Is<HomeworkStudent>(parameter =>
                    parameter.HomeworkId == query.HomeworkId
                    && parameter.StudentId == query.StudentId),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _homeworkFileRepositoryMock.Verify(
            repository => repository.ListFilesAsync(query.HomeworkId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_SubmittedHomeworkNotFound_ShouldReturnNullSubmittedHomeworkId()
    {
        // Arrange
        var query = _fixture.Create<GetStudentHomeworkQuery>();
        var studentHomework = _fixture.Build<StudentHomeworkInfo>()
            .With(result => result.Id, query.HomeworkId)
            .With(result => result.IsHomeworkSubmitted, false)
            .Create();

        SetupValidatorSuccess(query);
        SetupGetStudentHomework(query, studentHomework);
        SetupGetSubmittedHomeworkId(query, null);
        _homeworkFileRepositoryMock
            .Setup(repository => repository.ListFilesAsync(query.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var queryResponse = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        queryResponse.StudentHomeworkInfo.Should().BeEquivalentTo(studentHomework);
        queryResponse.SubmittedHomeworkId.Should().BeNull();
        queryResponse.StudentHomeworkInfo.IsHomeworkSubmitted.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_ValidationFailed_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetStudentHomeworkQuery>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

        // Act
        var act = () => _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
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
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyHomeworkRepository)
            .Returns(_homeworkRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySubmittedHomeworkRepository)
            .Returns(_submittedHomeworkRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyHomeworkFileRepository)
            .Returns(_homeworkFileRepositoryMock.Object);
    }

    private void SetupValidatorSuccess(GetStudentHomeworkQuery query)
    {
        _validatorMock
            .Setup(validator => validator.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private void SetupGetStudentHomework(GetStudentHomeworkQuery query, StudentHomeworkInfo studentHomeworkInfo)
    {
        _homeworkRepositoryMock
            .Setup(repository => repository.GetStudentHomeworkInfoAsync(
                It.Is<HomeworkStudent>(parameter => parameter.HomeworkId == query.HomeworkId && parameter.StudentId == query.StudentId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentHomeworkInfo);
    }

    private void SetupGetSubmittedHomeworkId(GetStudentHomeworkQuery query, SubmittedHomeworkId? submittedHomeworkId)
    {
        _submittedHomeworkRepositoryMock
            .Setup(repository => repository.GetSubmittedHomeworkIdAsync(
                It.Is<HomeworkStudent>(parameter => parameter.HomeworkId == query.HomeworkId && parameter.StudentId == query.StudentId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomeworkId);
    }
}
