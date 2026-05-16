using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetTeacherHomework;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Homeworks;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Homeworks.GetTeacherHomework;

public sealed class GetTeacherHomeworkHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlyHomeworkRepository> _homeworkRepositoryMock = new();
    private readonly Mock<IReadOnlySubmittedHomeworkRepository> _submittedHomeworkRepositoryMock = new();
    private readonly Mock<IReadOnlyHomeworkFileRepository> _homeworkFileRepositoryMock = new();
    private readonly Mock<IQueryValidator<GetTeacherHomeworkQuery, GetTeacherHomeworkQueryResponse>> _validatorMock = new();

    private readonly Fixture _fixture = new();
    private readonly GetTeacherHomeworkHandler _handler;

    public GetTeacherHomeworkHandlerTests()
    {
        SetupUnitOfWorkFactory();
        _handler = new GetTeacherHomeworkHandler(_unitOfWorkFactoryMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_PublishedHomework_ShouldReturnTeacherHomework()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherHomeworkQuery>();
        var teacherHomework = _fixture.Build<TeacherHomeworkInfo>()
            .With(result => result.Id, query.HomeworkId)
            .With(result => result.Status, HomeworkStatus.Published)
            .Create();
        var files = new[]
        {
            _fixture.Build<File>().With(result => result.StorageId, (StorageId)Guid.NewGuid()).Create(),
            _fixture.Build<File>().With(result => result.StorageId, (StorageId)Guid.NewGuid()).Create()
        };
        var submittedHomeworkCount = _fixture.Create<int>();

        SetupValidatorSuccess(query);
        SetupGetTeacherHomework(query, teacherHomework);
        SetupListFiles(query, files);
        _submittedHomeworkRepositoryMock
            .Setup(repository => repository.GetCountAsync(query.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomeworkCount);

        // Act
        var queryResponse = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        queryResponse.TeacherHomeworkInfo.Should().BeEquivalentTo(teacherHomework);
        queryResponse.Files.Should().BeEquivalentTo(files);
        queryResponse.SubmittedHomeworkCount.Should().Be(submittedHomeworkCount);
        _validatorMock.Verify(
            validator => validator.ValidateAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
        _homeworkRepositoryMock.Verify(
            repository => repository.GetTeacherHomeworkInfoAsync(query.HomeworkId, It.IsAny<CancellationToken>()),
            Times.Once);
        _homeworkFileRepositoryMock.Verify(
            repository => repository.ListFilesAsync(query.HomeworkId, It.IsAny<CancellationToken>()),
            Times.Once);
        _submittedHomeworkRepositoryMock.Verify(
            repository => repository.GetCountAsync(query.HomeworkId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DraftHomework_ShouldNotReturnSubmittedHomeworkCount()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherHomeworkQuery>();
        var teacherHomework = _fixture.Build<TeacherHomeworkInfo>()
            .With(result => result.Id, query.HomeworkId)
            .With(result => result.Status, HomeworkStatus.Draft)
            .Create();

        SetupValidatorSuccess(query);
        SetupGetTeacherHomework(query, teacherHomework);
        SetupListFiles(query, []);

        // Act
        var queryResponse = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        queryResponse.TeacherHomeworkInfo.Should().BeEquivalentTo(teacherHomework);
        queryResponse.Files.Should().BeEmpty();
        queryResponse.SubmittedHomeworkCount.Should().BeNull();
        _submittedHomeworkRepositoryMock.Verify(
            repository => repository.GetCountAsync(It.IsAny<HomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationFailed_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherHomeworkQuery>();

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

    private void SetupValidatorSuccess(GetTeacherHomeworkQuery query)
    {
        _validatorMock
            .Setup(validator => validator.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private void SetupGetTeacherHomework(GetTeacherHomeworkQuery query, TeacherHomeworkInfo teacherHomework)
    {
        _homeworkRepositoryMock
            .Setup(repository => repository.GetTeacherHomeworkInfoAsync(query.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacherHomework);
    }

    private void SetupListFiles(GetTeacherHomeworkQuery query, File[] files)
    {
        _homeworkFileRepositoryMock
            .Setup(repository => repository.ListFilesAsync(query.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(files);
    }
}
