using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetTeacherSubmittedHomework;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Students;
using Peerly.Core.Models.Submissions;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.GetTeacherSubmittedHomework;

public sealed class GetTeacherSubmittedHomeworkHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlySubmittedHomeworkRepository> _submittedHomeworkRepositoryMock = new();
    private readonly Mock<IReadOnlySubmittedHomeworkFileRepository> _submittedHomeworkFileRepositoryMock = new();
    private readonly Mock<IReadOnlySubmittedReviewRepository> _submittedReviewRepositoryMock = new();
    private readonly Mock<IReadOnlyStudentRepository> _studentRepositoryMock = new();
    private readonly Mock<IReadOnlyHomeworkRepository> _homeworkRepositoryMock = new();
    private readonly Mock<IReadOnlySubmittedHomeworkMarkRepository> _submittedHomeworkMarkRepositoryMock = new();
    private readonly Mock<IQueryValidator<GetTeacherSubmittedHomeworkQuery, GetTeacherSubmittedHomeworkQueryResponse>> _validatorMock = new();

    private readonly Fixture _fixture = new();
    private readonly GetTeacherSubmittedHomeworkHandler _handler;

    public GetTeacherSubmittedHomeworkHandlerTests()
    {
        SetupUnitOfWorkFactory();
        _handler = new GetTeacherSubmittedHomeworkHandler(_unitOfWorkFactoryMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationSuccess_ShouldReturnSubmittedHomework()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherSubmittedHomeworkQuery>();
        var submittedHomework = SetupSubmittedHomework(query);
        var owner = SetupStudent(submittedHomework.StudentId);
        var reviewer = SetupStudent();
        var files = new[]
        {
            _fixture.Build<File>().With(result => result.StorageId, (StorageId)Guid.NewGuid()).Create(),
            _fixture.Build<File>().With(result => result.StorageId, (StorageId)Guid.NewGuid()).Create()
        };
        var reviews = new[]
        {
            _fixture.Build<SubmittedReview>()
                .With(result => result.SubmittedHomeworkId, query.SubmittedHomeworkId)
                .With(result => result.StudentId, reviewer.Id)
                .Create()
        };
        var submittedHomeworkMark = _fixture.Build<SubmittedHomeworkMark>()
            .With(result => result.SubmittedHomeworkId, query.SubmittedHomeworkId)
            .With(result => result.ReviewersMark, 82)
            .With(result => result.TeacherMark, 91)
            .Create();

        SetupValidatorSuccess(query);
        SetupFiles(query, files);
        SetupReviews(query, reviews);
        SetupStudents(owner, reviewer);
        SetupHomework(submittedHomework.HomeworkId, HomeworkStatus.Finished);
        SetupSubmittedHomeworkMark(query, submittedHomeworkMark);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        response.SubmittedHomework.Should().Be(submittedHomework);
        response.Student.Should().Be(owner);
        response.Files.Should().BeEquivalentTo(files);
        response.SubmittedReviews.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Review = reviews[0],
            Reviewer = reviewer
        });
        response.ReviewersMark.Should().Be(submittedHomeworkMark.ReviewersMark);
        response.TeacherMark.Should().Be(submittedHomeworkMark.TeacherMark);
        _validatorMock.Verify(
            validator => validator.ValidateAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_HomeworkReviewing_ShouldNotReturnMarks()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherSubmittedHomeworkQuery>();
        var submittedHomework = SetupSubmittedHomework(query);
        var owner = SetupStudent(submittedHomework.StudentId);

        SetupValidatorSuccess(query);
        SetupFiles(query, []);
        SetupReviews(query, []);
        SetupStudents(owner);
        SetupHomework(submittedHomework.HomeworkId, HomeworkStatus.Reviewing);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        response.ReviewersMark.Should().BeNull();
        response.TeacherMark.Should().BeNull();
        _submittedHomeworkMarkRepositoryMock.Verify(
            repository => repository.GetBySubmittedHomeworkAsync(It.IsAny<SubmittedHomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_MarkNotFound_ShouldReturnNullMarks()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherSubmittedHomeworkQuery>();
        var submittedHomework = SetupSubmittedHomework(query);
        var owner = SetupStudent(submittedHomework.StudentId);

        SetupValidatorSuccess(query);
        SetupFiles(query, []);
        SetupReviews(query, []);
        SetupStudents(owner);
        SetupHomework(submittedHomework.HomeworkId, HomeworkStatus.Confirmation);
        SetupSubmittedHomeworkMark(query, null);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        response.ReviewersMark.Should().BeNull();
        response.TeacherMark.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ValidationFailed_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<GetTeacherSubmittedHomeworkQuery>();
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
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySubmittedHomeworkFileRepository)
            .Returns(_submittedHomeworkFileRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySubmittedReviewRepository)
            .Returns(_submittedReviewRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyStudentRepository)
            .Returns(_studentRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyHomeworkRepository)
            .Returns(_homeworkRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySubmittedHomeworkMarkRepository)
            .Returns(_submittedHomeworkMarkRepositoryMock.Object);
    }

    private void SetupValidatorSuccess(GetTeacherSubmittedHomeworkQuery query)
    {
        _validatorMock
            .Setup(validator => validator.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private SubmittedHomework SetupSubmittedHomework(GetTeacherSubmittedHomeworkQuery query)
    {
        var submittedHomework = _fixture.Build<SubmittedHomework>()
            .With(result => result.Id, query.SubmittedHomeworkId)
            .Create();
        _submittedHomeworkRepositoryMock
            .Setup(repository => repository.GetAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomework);

        return submittedHomework;
    }

    private Student SetupStudent(StudentId? studentId = null)
    {
        return _fixture.Build<Student>()
            .With(result => result.Id, studentId ?? _fixture.Create<StudentId>())
            .Create();
    }

    private void SetupFiles(GetTeacherSubmittedHomeworkQuery query, File[] files)
    {
        _submittedHomeworkFileRepositoryMock
            .Setup(repository => repository.ListBySubmittedHomeworkAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(files);
    }

    private void SetupReviews(GetTeacherSubmittedHomeworkQuery query, SubmittedReview[] reviews)
    {
        _submittedReviewRepositoryMock
            .Setup(repository => repository.ListBySubmittedHomeworkAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviews);
    }

    private void SetupStudents(params Student[] students)
    {
        _studentRepositoryMock
            .Setup(repository => repository.ListAsync(It.IsAny<StudentFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
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

    private void SetupSubmittedHomeworkMark(GetTeacherSubmittedHomeworkQuery query, SubmittedHomeworkMark? mark)
    {
        _submittedHomeworkMarkRepositoryMock
            .Setup(repository => repository.GetBySubmittedHomeworkAsync(query.SubmittedHomeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mark);
    }
}
