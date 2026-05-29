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

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.SearchTeacherHomeworks;

public sealed class SearchTeacherHomeworksIntegrationTests : SearchTeacherHomeworksIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public SearchTeacherHomeworksIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1SearchTeacherHomeworks_TeacherHasHomeworks_ShouldReturnAccessibleTeacherHomeworks()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var otherTeacherId = teacherId + 1;
        await AddTeacherInDbAsync(teacherId);
        await AddTeacherInDbAsync(otherTeacherId);

        var courseId = await AddCourseInDbAsync();
        var groupInCourseId = await AddGroupInDbAsync(courseId);
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        var groupCourseId = await AddCourseInDbAsync();
        var groupId = await AddGroupInDbAsync(groupCourseId);
        var otherGroupId = await AddGroupInDbAsync(groupCourseId);
        await AddGroupTeacherInDbAsync(groupId, teacherId);

        var otherCourseId = await AddCourseInDbAsync();
        await AddCourseTeacherInDbAsync(otherCourseId, otherTeacherId);

        var rubricId = await AddRubricInDbAsync(teacherId);
        var homeworkName = _fixture.Create<string>();
        var homeworkDescription = _fixture.Create<string>();
        var deadline = DateTimeOffset.UtcNow.AddDays(10);
        var reviewDeadline = DateTimeOffset.UtcNow.AddDays(15);
        var courseHomeworkId = await AddHomeworkInDbAsync(
            courseId,
            teacherId,
            HomeworkStatusModel.Published,
            name: homeworkName,
            description: homeworkDescription,
            deadline: deadline,
            reviewDeadline: reviewDeadline,
            amountOfReviewers: 4,
            discrepancyThreshold: 20,
            rubricId: rubricId);
        var courseGroupHomeworkId = await AddHomeworkInDbAsync(
            courseId,
            teacherId,
            HomeworkStatusModel.Draft,
            groupId: groupInCourseId,
            deadline: DateTimeOffset.UtcNow.AddDays(9));
        var groupHomeworkId = await AddHomeworkInDbAsync(
            groupCourseId,
            otherTeacherId,
            HomeworkStatusModel.Reviewing,
            groupId: groupId,
            deadline: DateTimeOffset.UtcNow.AddDays(8));
        await AddHomeworkInDbAsync(
            groupCourseId,
            otherTeacherId,
            HomeworkStatusModel.Published,
            deadline: DateTimeOffset.UtcNow.AddDays(12));
        await AddHomeworkInDbAsync(
            groupCourseId,
            otherTeacherId,
            HomeworkStatusModel.Published,
            groupId: otherGroupId,
            deadline: DateTimeOffset.UtcNow.AddDays(11));
        await AddHomeworkInDbAsync(
            otherCourseId,
            otherTeacherId,
            HomeworkStatusModel.Published,
            deadline: DateTimeOffset.UtcNow.AddDays(13));

        var request = new V1SearchTeacherHomeworksRequest
        {
            TeacherId = teacherId,
            Filter = new SearchHomeworksFilter(),
            PaginationInfo = new PaginationInfo
            {
                Offset = 0,
                PageSize = 10
            }
        };

        // Act
        var response = await SearchTeacherHomeworksClient.V1SearchTeacherHomeworksAsync(request);

        // Assert
        response.TeacherHomeworkInfos.Select(homeworkInfo => homeworkInfo.Id)
            .Should()
            .BeEquivalentTo([courseHomeworkId, courseGroupHomeworkId, groupHomeworkId]);

        var courseHomeworkInfo = response.TeacherHomeworkInfos.Single(homeworkInfo => homeworkInfo.Id == courseHomeworkId);
        courseHomeworkInfo.Name.Should().Be(homeworkName);
        courseHomeworkInfo.Description.Should().Be(homeworkDescription);
        courseHomeworkInfo.Status.Should().Be(ProtoHomeworkStatus.Published);
        courseHomeworkInfo.Deadline.ToDateTimeOffset().Should().BeCloseTo(deadline, TimeSpan.FromMilliseconds(1));
        courseHomeworkInfo.ReviewDeadline.ToDateTimeOffset().Should().BeCloseTo(reviewDeadline, TimeSpan.FromMilliseconds(1));
        courseHomeworkInfo.AmountOfReviewers.Should().Be(4);
        courseHomeworkInfo.DiscrepancyThreshold.Should().Be(20);
        courseHomeworkInfo.HasRubricId.Should().BeTrue();
        courseHomeworkInfo.RubricId.Should().Be(rubricId);

        response.TeacherHomeworkInfos.Single(homeworkInfo => homeworkInfo.Id == courseGroupHomeworkId)
            .Status.Should().Be(ProtoHomeworkStatus.Draft);
        response.TeacherHomeworkInfos.Single(homeworkInfo => homeworkInfo.Id == groupHomeworkId)
            .Status.Should().Be(ProtoHomeworkStatus.Reviewing);
    }

    [Fact]
    public async Task V1SearchTeacherHomeworks_StatusFilter_ShouldReturnOnlyRequestedStatuses()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        await AddTeacherInDbAsync(teacherId);

        var courseId = await AddCourseInDbAsync();
        await AddCourseTeacherInDbAsync(courseId, teacherId);

        var draftHomeworkId = await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Draft);
        await AddHomeworkInDbAsync(courseId, teacherId, HomeworkStatusModel.Published);

        var request = new V1SearchTeacherHomeworksRequest
        {
            TeacherId = teacherId,
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
        var response = await SearchTeacherHomeworksClient.V1SearchTeacherHomeworksAsync(request);

        // Assert
        response.TeacherHomeworkInfos.Select(homeworkInfo => homeworkInfo.Id).Should().Equal([draftHomeworkId]);
        response.TeacherHomeworkInfos.Single().Status.Should().Be(ProtoHomeworkStatus.Draft);
    }

    [Fact]
    public async Task V1SearchTeacherHomeworks_Pagination_ShouldReturnRequestedPage()
    {
        // Arrange
        const int PageSize = 2;
        var teacherId = _fixture.Create<long>();
        await AddTeacherInDbAsync(teacherId);

        var courseId = await AddCourseInDbAsync();
        await AddCourseTeacherInDbAsync(courseId, teacherId);

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

        var request = new V1SearchTeacherHomeworksRequest
        {
            TeacherId = teacherId,
            Filter = new SearchHomeworksFilter(),
            PaginationInfo = new PaginationInfo
            {
                Offset = 1,
                PageSize = PageSize
            }
        };

        // Act
        var response = await SearchTeacherHomeworksClient.V1SearchTeacherHomeworksAsync(request);

        // Assert
        response.TeacherHomeworkInfos.Select(homeworkInfo => homeworkInfo.Id).Should().Equal([thirdHomeworkId, secondHomeworkId]);
        response.TeacherHomeworkInfos.Should().HaveCount(PageSize);
    }

    [Fact]
    public async Task V1SearchTeacherHomeworks_HomeworksNotFound_ShouldReturnEmptyTeacherHomeworkInfos()
    {
        // Arrange
        var request = new V1SearchTeacherHomeworksRequest
        {
            TeacherId = _fixture.Create<long>(),
            Filter = new SearchHomeworksFilter(),
            PaginationInfo = new PaginationInfo
            {
                Offset = 0,
                PageSize = 10
            }
        };

        // Act
        var response = await SearchTeacherHomeworksClient.V1SearchTeacherHomeworksAsync(request);

        // Assert
        response.TeacherHomeworkInfos.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1SearchTeacherHomeworks_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = new V1SearchTeacherHomeworksRequest
        {
            TeacherId = teacherId,
            Filter = new SearchHomeworksFilter(),
            PaginationInfo = new PaginationInfo
            {
                Offset = 0,
                PageSize = 10
            }
        };

        // Act
        var act = async () => await SearchTeacherHomeworksClient.V1SearchTeacherHomeworksAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }

    [Fact]
    public async Task V1SearchTeacherHomeworks_NegativeOffset_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = new V1SearchTeacherHomeworksRequest
        {
            TeacherId = _fixture.Create<long>(),
            Filter = new SearchHomeworksFilter(),
            PaginationInfo = new PaginationInfo
            {
                Offset = -1,
                PageSize = 10
            }
        };

        // Act
        var act = async () => await SearchTeacherHomeworksClient.V1SearchTeacherHomeworksAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.PaginationInfo.Offset));
    }

    [Fact]
    public async Task V1SearchTeacherHomeworks_NegativePageSize_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = new V1SearchTeacherHomeworksRequest
        {
            TeacherId = _fixture.Create<long>(),
            Filter = new SearchHomeworksFilter(),
            PaginationInfo = new PaginationInfo
            {
                Offset = 0,
                PageSize = -1
            }
        };

        // Act
        var act = async () => await SearchTeacherHomeworksClient.V1SearchTeacherHomeworksAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.PaginationInfo.PageSize));
    }
}
