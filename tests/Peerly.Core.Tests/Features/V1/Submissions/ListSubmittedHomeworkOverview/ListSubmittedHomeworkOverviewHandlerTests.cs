using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.ListSubmittedHomeworkOverview;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Students;
using Peerly.Core.Models.Submissions;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Submissions.ListSubmittedHomeworkOverview;

public sealed class ListSubmittedHomeworkOverviewHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlySubmittedHomeworkRepository> _submittedHomeworkRepositoryMock = new();
    private readonly Mock<IReadOnlyHomeworkRepository> _homeworkRepositoryMock = new();
    private readonly Mock<IReadOnlySubmittedHomeworkMarkRepository> _submittedHomeworkMarkRepositoryMock = new();
    private readonly Mock<IReadOnlySubmittedReviewRepository> _submittedReviewRepositoryMock = new();
    private readonly Mock<IReadOnlyStudentRepository> _studentRepositoryMock = new();
    private readonly Mock<IQueryValidator<ListSubmittedHomeworkOverviewQuery, ListSubmittedHomeworkOverviewQueryResponse>> _validatorMock = new();
    private readonly Fixture _fixture = new();
    private readonly ListSubmittedHomeworkOverviewHandler _handler;

    public ListSubmittedHomeworkOverviewHandlerTests()
    {
        SetupUnitOfWorkFactory();
        _handler = new ListSubmittedHomeworkOverviewHandler(_unitOfWorkFactoryMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationSuccess_ShouldReturnSubmittedHomeworkOverviews()
    {
        // Arrange
        var query = _fixture.Create<ListSubmittedHomeworkOverviewQuery>();
        var firstStudent = SetupStudent();
        var secondStudent = SetupStudent();
        var submittedHomeworkStudents = new[]
        {
            SetupSubmittedHomeworkStudent(firstStudent.Id),
            SetupSubmittedHomeworkStudent(secondStudent.Id)
        };
        var mark = _fixture.Build<SubmittedHomeworkMark>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkStudents[0].SubmittedHomeworkId)
            .With(result => result.ReviewersMark, 82)
            .With(result => result.TeacherMark, 95)
            .With(result => result.HasDiscrepancy, true)
            .Create();
        var submittedReviewMarks = new[]
        {
            SetupSubmittedReviewMark(submittedHomeworkStudents[0].SubmittedHomeworkId),
            SetupSubmittedReviewMark(submittedHomeworkStudents[0].SubmittedHomeworkId),
            SetupSubmittedReviewMark(submittedHomeworkStudents[1].SubmittedHomeworkId)
        };

        SetupValidatorSuccess(query);
        SetupSubmittedHomeworkStudents(query.HomeworkId, submittedHomeworkStudents);
        SetupHomework(query.HomeworkId, HomeworkStatus.Finished);
        SetupSubmittedHomeworkMarks(query.HomeworkId, [mark]);
        SetupSubmittedReviewMarks(query.HomeworkId, submittedReviewMarks);
        SetupStudents(firstStudent, secondStudent);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        var overviews = response.SubmittedHomeworkOverviews.ToArray();
        overviews.Should().HaveCount(2);
        overviews[0].Should().BeEquivalentTo(new
        {
            submittedHomeworkStudents[0].SubmittedHomeworkId,
            Student = firstStudent,
            ReviewCount = 2,
            mark.ReviewersMark,
            mark.HasDiscrepancy,
            mark.TeacherMark
        });
        overviews[1].Should().BeEquivalentTo(new
        {
            submittedHomeworkStudents[1].SubmittedHomeworkId,
            Student = secondStudent,
            ReviewCount = 1,
            ReviewersMark = (int?)null,
            HasDiscrepancy = (bool?)null,
            TeacherMark = (int?)null
        });
        _validatorMock.Verify(
            validator => validator.ValidateAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_HomeworkReviewing_ShouldNotReturnMarks()
    {
        // Arrange
        var query = _fixture.Create<ListSubmittedHomeworkOverviewQuery>();
        var student = SetupStudent();
        var submittedHomeworkStudent = SetupSubmittedHomeworkStudent(student.Id);

        SetupValidatorSuccess(query);
        SetupSubmittedHomeworkStudents(query.HomeworkId, [submittedHomeworkStudent]);
        SetupHomework(query.HomeworkId, HomeworkStatus.Reviewing);
        SetupSubmittedReviewMarks(query.HomeworkId, [SetupSubmittedReviewMark(submittedHomeworkStudent.SubmittedHomeworkId)]);
        SetupStudents(student);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        var overview = response.SubmittedHomeworkOverviews.Should().ContainSingle().Which;
        overview.ReviewCount.Should().Be(1);
        overview.ReviewersMark.Should().BeNull();
        overview.HasDiscrepancy.Should().BeNull();
        overview.TeacherMark.Should().BeNull();
        _submittedHomeworkMarkRepositoryMock.Verify(
            repository => repository.ListAsync(It.IsAny<HomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_SubmittedHomeworkStudentsEmpty_ShouldReturnEmptyCollection()
    {
        // Arrange
        var query = _fixture.Create<ListSubmittedHomeworkOverviewQuery>();

        SetupValidatorSuccess(query);
        SetupSubmittedHomeworkStudents(query.HomeworkId, []);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        response.SubmittedHomeworkOverviews.Should().BeEmpty();
        _homeworkRepositoryMock.Verify(
            repository => repository.GetAsync(It.IsAny<HomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _submittedReviewRepositoryMock.Verify(
            repository => repository.ListSubmittedReviewMarksAsync(It.IsAny<HomeworkId>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _studentRepositoryMock.Verify(
            repository => repository.ListAsync(It.IsAny<StudentFilter>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationFailed_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = _fixture.Create<ListSubmittedHomeworkOverviewQuery>();
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
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySubmittedHomeworkMarkRepository)
            .Returns(_submittedHomeworkMarkRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySubmittedReviewRepository)
            .Returns(_submittedReviewRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyStudentRepository)
            .Returns(_studentRepositoryMock.Object);
    }

    private void SetupValidatorSuccess(ListSubmittedHomeworkOverviewQuery query)
    {
        _validatorMock
            .Setup(validator => validator.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private Student SetupStudent()
    {
        return _fixture.Create<Student>();
    }

    private SubmittedHomeworkStudent SetupSubmittedHomeworkStudent(StudentId studentId)
    {
        return _fixture.Build<SubmittedHomeworkStudent>()
            .With(result => result.StudentId, studentId)
            .Create();
    }

    private SubmittedHomeworkReviewerMark SetupSubmittedReviewMark(SubmittedHomeworkId submittedHomeworkId)
    {
        return _fixture.Build<SubmittedHomeworkReviewerMark>()
            .With(result => result.SubmittedHomeworkId, submittedHomeworkId)
            .Create();
    }

    private void SetupSubmittedHomeworkStudents(HomeworkId homeworkId, SubmittedHomeworkStudent[] submittedHomeworkStudents)
    {
        _submittedHomeworkRepositoryMock
            .Setup(repository => repository.ListSubmittedHomeworkStudentAsync(
                It.Is<SubmittedHomeworkFilter>(filter => filter.HomeworkIds.Single() == homeworkId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedHomeworkStudents);
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

    private void SetupSubmittedHomeworkMarks(HomeworkId homeworkId, SubmittedHomeworkMark[] marks)
    {
        _submittedHomeworkMarkRepositoryMock
            .Setup(repository => repository.ListAsync(homeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(marks);
    }

    private void SetupSubmittedReviewMarks(HomeworkId homeworkId, SubmittedHomeworkReviewerMark[] marks)
    {
        _submittedReviewRepositoryMock
            .Setup(repository => repository.ListSubmittedReviewMarksAsync(homeworkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(marks);
    }

    private void SetupStudents(params Student[] students)
    {
        _studentRepositoryMock
            .Setup(repository => repository.ListAsync(It.IsAny<StudentFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
    }
}
