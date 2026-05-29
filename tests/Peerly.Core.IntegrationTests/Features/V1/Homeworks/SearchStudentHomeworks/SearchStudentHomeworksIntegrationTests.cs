using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;
using HomeworkStatusModel = Peerly.Core.Models.Homeworks.HomeworkStatus;
using ProtoHomeworkStatus = Peerly.Core.V1.HomeworkStatus;

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.SearchStudentHomeworks;

public sealed class SearchStudentHomeworksIntegrationTests : SearchStudentHomeworksIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public SearchStudentHomeworksIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1SearchStudentHomeworks_StudentHasHomeworks_ShouldReturnVisibleStudentHomeworks()
    {
        // Arrange
        var studentId = _fixture.Create<long>();
        var otherStudentId = studentId + 1;
        var teacherId = _fixture.Create<long>();
        await AddStudentInDbAsync(studentId);
        await AddStudentInDbAsync(otherStudentId);
        await AddTeacherInDbAsync(teacherId);

        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);
        await AddGroupStudentInDbAsync(groupId, studentId);

        var otherCourseId = await AddCourseInDbAsync();
        var otherGroupId = await AddGroupInDbAsync(otherCourseId);
        await AddGroupStudentInDbAsync(otherGroupId, otherStudentId);

        var rubricId = await AddRubricInDbAsync(teacherId);
        var publishedHomeworkName = _fixture.Create<string>();
        var publishedHomeworkDescription = _fixture.Create<string>();
        var publishedHomeworkDeadline = DateTimeOffset.UtcNow.AddDays(10);
        var publishedHomeworkReviewDeadline = DateTimeOffset.UtcNow.AddDays(15);
        var publishedHomeworkId = await AddHomeworkInDbAsync(
            courseId,
            teacherId,
            HomeworkStatusModel.Published,
            name: publishedHomeworkName,
            description: publishedHomeworkDescription,
            deadline: publishedHomeworkDeadline,
            reviewDeadline: publishedHomeworkReviewDeadline,
            amountOfReviewers: 4,
            rubricId: rubricId);
        var reviewingHomeworkId = await AddHomeworkInDbAsync(
            courseId,
            teacherId,
            HomeworkStatusModel.Reviewing,
            groupId: groupId,
            deadline: DateTimeOffset.UtcNow.AddDays(5));
        await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Draft);
        await AddHomeworkInDbAsync(otherCourseId, teacherId, HomeworkStatusModel.Published);
        await AddSubmittedHomeworkInDbAsync(publishedHomeworkId, studentId);

        var request = new V1SearchStudentHomeworksRequest
        {
            StudentId = studentId,
            Filter = new SearchHomeworksFilter(),
            PaginationInfo = new PaginationInfo
            {
                Offset = 0,
                PageSize = 10
            }
        };

        // Act
        var response = await SearchStudentHomeworksClient.V1SearchStudentHomeworksAsync(request);

        // Assert
        response.StudentHomeworkInfos.Select(homeworkInfo => homeworkInfo.Id)
            .Should()
            .BeEquivalentTo([publishedHomeworkId, reviewingHomeworkId]);

        var publishedHomeworkInfo = response.StudentHomeworkInfos.Single(homeworkInfo => homeworkInfo.Id == publishedHomeworkId);
        publishedHomeworkInfo.Name.Should().Be(publishedHomeworkName);
        publishedHomeworkInfo.Description.Should().Be(publishedHomeworkDescription);
        publishedHomeworkInfo.Status.Should().Be(ProtoHomeworkStatus.Published);
        publishedHomeworkInfo.AmountOfReviewers.Should().Be(4);
        publishedHomeworkInfo.HasRubricId.Should().BeTrue();
        publishedHomeworkInfo.RubricId.Should().Be(rubricId);
        publishedHomeworkInfo.IsHomeworkSubmitted.Should().BeTrue();

        var reviewingHomeworkInfo = response.StudentHomeworkInfos.Single(homeworkInfo => homeworkInfo.Id == reviewingHomeworkId);
        reviewingHomeworkInfo.Status.Should().Be(ProtoHomeworkStatus.Reviewing);
        reviewingHomeworkInfo.IsHomeworkSubmitted.Should().BeFalse();
    }

    [Fact]
    public async Task V1SearchStudentHomeworks_StatusFilter_ShouldReturnOnlyRequestedStatuses()
    {
        // Arrange
        var studentId = _fixture.Create<long>();
        var teacherId = _fixture.Create<long>();
        await AddStudentInDbAsync(studentId);
        await AddTeacherInDbAsync(teacherId);

        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);
        await AddGroupStudentInDbAsync(groupId, studentId);

        var publishedHomeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published);
        await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Reviewing);

        var request = new V1SearchStudentHomeworksRequest
        {
            StudentId = studentId,
            Filter = new SearchHomeworksFilter
            {
                HomeworkStatuses = { ProtoHomeworkStatus.Published }
            },
            PaginationInfo = new PaginationInfo
            {
                Offset = 0,
                PageSize = 10
            }
        };

        // Act
        var response = await SearchStudentHomeworksClient.V1SearchStudentHomeworksAsync(request);

        // Assert
        response.StudentHomeworkInfos.Select(homeworkInfo => homeworkInfo.Id).Should().Equal([publishedHomeworkId]);
        response.StudentHomeworkInfos.Single().Status.Should().Be(ProtoHomeworkStatus.Published);
    }

    [Fact]
    public async Task V1SearchStudentHomeworks_Pagination_ShouldReturnRequestedPage()
    {
        // Arrange
        const int PageSize = 2;
        var studentId = _fixture.Create<long>();
        var teacherId = _fixture.Create<long>();
        await AddStudentInDbAsync(studentId);
        await AddTeacherInDbAsync(teacherId);

        var courseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(courseId);
        await AddGroupStudentInDbAsync(groupId, studentId);

        await AddHomeworkInDbAsync(
            courseId,
            teacherId,
            HomeworkStatusModel.Published,
            deadline: DateTimeOffset.UtcNow.AddDays(1));
        var secondHomeworkId = await AddHomeworkInDbAsync(
            courseId,
            teacherId,
            HomeworkStatusModel.Published,
            deadline: DateTimeOffset.UtcNow.AddDays(2));
        var thirdHomeworkId = await AddHomeworkInDbAsync(
            courseId,
            teacherId,
            HomeworkStatusModel.Published,
            deadline: DateTimeOffset.UtcNow.AddDays(3));
        await AddHomeworkInDbAsync(
            courseId,
            teacherId,
            HomeworkStatusModel.Published,
            deadline: DateTimeOffset.UtcNow.AddDays(4));

        var request = new V1SearchStudentHomeworksRequest
        {
            StudentId = studentId,
            Filter = new SearchHomeworksFilter(),
            PaginationInfo = new PaginationInfo
            {
                Offset = 1,
                PageSize = PageSize
            }
        };

        // Act
        var response = await SearchStudentHomeworksClient.V1SearchStudentHomeworksAsync(request);

        // Assert
        response.StudentHomeworkInfos.Select(homeworkInfo => homeworkInfo.Id).Should().Equal([thirdHomeworkId, secondHomeworkId]);
        response.StudentHomeworkInfos.Should().HaveCount(PageSize);
    }

    [Fact]
    public async Task V1SearchStudentHomeworks_HomeworksNotFound_ShouldReturnEmptyStudentHomeworkInfos()
    {
        // Arrange
        var request = new V1SearchStudentHomeworksRequest
        {
            StudentId = _fixture.Create<long>(),
            Filter = new SearchHomeworksFilter(),
            PaginationInfo = new PaginationInfo
            {
                Offset = 0,
                PageSize = 10
            }
        };

        // Act
        var response = await SearchStudentHomeworksClient.V1SearchStudentHomeworksAsync(request);

        // Assert
        response.StudentHomeworkInfos.Should().BeEmpty();
    }

    [Fact]
    public async Task V1SearchStudentHomeworks_DraftStatusInFilter_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = new V1SearchStudentHomeworksRequest
        {
            StudentId = _fixture.Create<long>(),
            Filter = new SearchHomeworksFilter
            {
                HomeworkStatuses = { ProtoHomeworkStatus.Draft }
            },
            PaginationInfo = new PaginationInfo
            {
                Offset = 0,
                PageSize = 10
            }
        };

        // Act
        var act = async () => await SearchStudentHomeworksClient.V1SearchStudentHomeworksAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Filter.HomeworkStatuses));
    }
}
