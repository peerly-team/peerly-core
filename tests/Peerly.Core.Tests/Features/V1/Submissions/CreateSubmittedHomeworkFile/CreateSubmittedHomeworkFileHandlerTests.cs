using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.ApplicationServices.Services.Anonymization.Abstractions;
using Peerly.Core.ApplicationServices.Services.Anonymization.Models;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Students;
using Peerly.Core.Models.Submissions;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.CreateSubmittedHomeworkFile;

public sealed class CreateSubmittedHomeworkFileHandlerTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ISubmittedHomeworkFileRepository> _submittedHomeworkFileRepositoryMock = new();
    private readonly Mock<IFileAnonymizationService> _anonymizationServiceMock = new();
    private readonly Mock<ICommandValidator<CreateSubmittedHomeworkFileCommand, CreateSubmittedHomeworkFileCommandResponse>> _validatorMock = new();
    private readonly Mock<IClock> _clockMock = new();

    private readonly Fixture _fixture = new();
    private readonly CreateSubmittedHomeworkFileHandler _handler;

    public CreateSubmittedHomeworkFileHandlerTests()
    {
        _fixture.Customize(new SupportMutableValueTypesCustomization());

        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _clockMock
            .Setup(clock => clock.GetCurrentTime())
            .Returns(_fixture.Create<DateTimeOffset>());

        _handler = new CreateSubmittedHomeworkFileHandler(
            unitOfWorkFactory,
            _anonymizationServiceMock.Object,
            _validatorMock.Object,
            _clockMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationSuccessWithAnonymization_ShouldAddFileAndAnonymizedFile()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkFileCommand>();
        var expectedFileId = _fixture.Create<FileId>();
        var expectedAnonymizedFileId = _fixture.Create<FileId>();

        SetupValidationSuccess(command);
        SetupRepositoriesForSuccess(command);

        var anonymizationResult = _fixture.Create<AnonymizationResult>();
        _anonymizationServiceMock
            .Setup(s => s.AnonymizeAsync(It.IsAny<AnonymizationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(anonymizationResult);

        _unitOfWorkMock
            .Setup(uow => uow.FileRepository.AddAsync(
                It.Is<FileAddItem>(item => item.StorageId == command.StorageId && item.Name == command.FileName && item.Size == command.FileSize),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFileId);
        _unitOfWorkMock
            .Setup(uow => uow.FileRepository.AddAsync(
                It.Is<FileAddItem>(item => item.StorageId == anonymizationResult.AnonymizedStorageId && item.Name == anonymizationResult.AnonymizedFileName && item.Size == anonymizationResult.Size),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAnonymizedFileId);

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT0.Should().BeTrue();
        response.AsT0.FileId.Should().Be(expectedFileId);

        _unitOfWorkMock.Verify(
            uow => uow.FileRepository.AddAsync(It.IsAny<FileAddItem>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _submittedHomeworkFileRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<SubmittedHomeworkFileAddItem>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationSuccessWithoutAnonymization_ShouldAddOnlyOriginalFile()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkFileCommand>();
        var expectedFileId = _fixture.Create<FileId>();

        SetupValidationSuccess(command);
        SetupRepositoriesForSuccess(command);

        _anonymizationServiceMock
            .Setup(s => s.AnonymizeAsync(It.IsAny<AnonymizationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AnonymizationResult?)null);

        _unitOfWorkMock
            .Setup(uow => uow.FileRepository.AddAsync(It.IsAny<FileAddItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFileId);

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT0.Should().BeTrue();
        response.AsT0.FileId.Should().Be(expectedFileId);

        _unitOfWorkMock.Verify(
            uow => uow.FileRepository.AddAsync(It.IsAny<FileAddItem>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _submittedHomeworkFileRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<SubmittedHomeworkFileAddItem>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultOtherError_ShouldBeOtherError()
    {
        // Arrange
        var command = _fixture.Create<CreateSubmittedHomeworkFileCommand>();

        _validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtherError.NotFound());

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT2.Should().BeTrue();
        response.AsT2.Type.Should().Be(ErrorType.NotFound);

        _unitOfWorkMock.Verify(
            uow => uow.FileRepository.AddAsync(It.IsAny<FileAddItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultValidationError_ShouldBeValidationError()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();
        var command = _fixture.Create<CreateSubmittedHomeworkFileCommand>();

        _validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationError.From(errorMessage));

        // Act
        var response = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        response.IsT1.Should().BeTrue();
        response.AsT1.Errors.Should().NotBeNull().And.ContainSingle(errorMessage);

        _unitOfWorkMock.Verify(
            uow => uow.FileRepository.AddAsync(It.IsAny<FileAddItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupValidationSuccess(CreateSubmittedHomeworkFileCommand command)
    {
        _validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok);
    }

    private void SetupRepositoriesForSuccess(CreateSubmittedHomeworkFileCommand command)
    {
        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(sh => sh.Id, command.SubmittedHomeworkId)
            .With(sh => sh.StudentId, command.StudentId)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.SubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomework);

        var homework = _fixture.Build<Homework>()
            .With(h => h.Id, submittedHomework.HomeworkId)
            .With(h => h.GroupId, (GroupId?)null)
            .Create();
        _unitOfWorkMock
            .Setup(uow => uow.HomeworkRepository.GetAsync(submittedHomework.HomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(homework);

        var groups = _fixture.CreateMany<Group>(1).ToArray();
        _unitOfWorkMock
            .Setup(uow => uow.GroupRepository.ListAsync(It.IsAny<GroupFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(groups);

        var groupStudents = _fixture.CreateMany<GroupStudent>(1).ToArray();
        _unitOfWorkMock
            .Setup(uow => uow.GroupStudentRepository.ListAsync(It.IsAny<GroupStudentFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(groupStudents);

        var students = _fixture.CreateMany<Student>(1).ToArray();
        _unitOfWorkMock
            .Setup(uow => uow.StudentRepository.ListAsync(It.IsAny<StudentFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        var unitOfWorkFactoryMock = new Mock<ICommonUnitOfWorkFactory>();
        unitOfWorkFactoryMock
            .Setup(f => f.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        var operationSetMock = new Mock<IOperationSet>();
        operationSetMock
            .Setup(operationSet => operationSet.Complete(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(uow => uow.StartOperationSet(It.IsAny<CancellationToken>()))
            .ReturnsAsync(operationSetMock.Object);
        _unitOfWorkMock
            .SetupGet(uow => uow.SubmittedHomeworkFileRepository)
            .Returns(_submittedHomeworkFileRepositoryMock.Object);

        return unitOfWorkFactoryMock.Object;
    }
}
