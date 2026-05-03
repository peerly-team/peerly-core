using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;
using CourseStatusModel = Peerly.Core.Models.Courses.CourseStatus;
using ProtoCourseStatus = Peerly.Core.V1.CourseStatus;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.SearchStudentCourses;

public sealed class SearchStudentCoursesIntegrationTests : SearchStudentCoursesIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public SearchStudentCoursesIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1SearchStudentCourses_StudentHasCourses_ShouldReturnFilteredNotDeletedCourses()
    {
        // Arrange
        var studentId = _fixture.Create<long>();
        var otherStudentId = _fixture.Create<long>();
        await AddStudentInDbAsync(studentId);
        await AddStudentInDbAsync(otherStudentId);

        var inProgressCourseName = _fixture.Create<string>();
        var inProgressCourseDescription = _fixture.Create<string>();
        var inProgressCourseId = await AddCourseInDbAsync(inProgressCourseName, inProgressCourseDescription, CourseStatusModel.InProgress);

        var finishedCourseName = _fixture.Create<string>();
        var finishedCourseDescription = _fixture.Create<string>();
        var finishedCourseId = await AddCourseInDbAsync(finishedCourseName, finishedCourseDescription, CourseStatusModel.Finished);

        var deletedCourseId = await AddCourseInDbAsync(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            CourseStatusModel.Deleted);
        var otherStudentCourseId = await AddCourseInDbAsync(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            CourseStatusModel.Finished);

        await AddGroupStudentInDbAsync(await AddGroupInDbAsync(inProgressCourseId, _fixture.Create<string>()), studentId);
        await AddGroupStudentInDbAsync(await AddGroupInDbAsync(finishedCourseId, _fixture.Create<string>()), studentId);
        await AddGroupStudentInDbAsync(await AddGroupInDbAsync(deletedCourseId, _fixture.Create<string>()), studentId);
        await AddGroupStudentInDbAsync(await AddGroupInDbAsync(otherStudentCourseId, _fixture.Create<string>()), otherStudentId);

        var request = new V1SearchStudentCoursesRequest
        {
            StudentId = studentId,
            Filter = new SearchCoursesFilter
            {
                CourseStatuses =
                {
                    ProtoCourseStatus.InProgress,
                    ProtoCourseStatus.Finished,
                    ProtoCourseStatus.Deleted
                }
            },
            PaginationInfo = new PaginationInfo
            {
                Offset = 0,
                PageSize = 10
            }
        };

        // Act
        var response = await SearchStudentCoursesClient.V1SearchStudentCoursesAsync(request);

        // Assert
        response.CourseInfos.Select(courseInfo => courseInfo.Id).Should().BeEquivalentTo([inProgressCourseId, finishedCourseId]);

        var inProgressCourseInfo = response.CourseInfos.Single(courseInfo => courseInfo.Id == inProgressCourseId);
        inProgressCourseInfo.Name.Should().Be(inProgressCourseName);
        inProgressCourseInfo.Description.Should().Be(inProgressCourseDescription);
        inProgressCourseInfo.Status.Should().Be(ProtoCourseStatus.InProgress);

        var finishedCourseInfo = response.CourseInfos.Single(courseInfo => courseInfo.Id == finishedCourseId);
        finishedCourseInfo.Name.Should().Be(finishedCourseName);
        finishedCourseInfo.Description.Should().Be(finishedCourseDescription);
        finishedCourseInfo.Status.Should().Be(ProtoCourseStatus.Finished);
    }

    [Fact]
    public async Task V1SearchStudentCourses_EmptyFilter_ShouldReturnAllNotDeletedStudentCourses()
    {
        // Arrange
        var studentId = _fixture.Create<long>();
        await AddStudentInDbAsync(studentId);

        var draftCourseId = await AddCourseInDbAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var inProgressCourseId = await AddCourseInDbAsync(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            CourseStatusModel.InProgress);
        var deletedCourseId = await AddCourseInDbAsync(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            CourseStatusModel.Deleted);

        await AddGroupStudentInDbAsync(await AddGroupInDbAsync(draftCourseId, _fixture.Create<string>()), studentId);
        await AddGroupStudentInDbAsync(await AddGroupInDbAsync(inProgressCourseId, _fixture.Create<string>()), studentId);
        await AddGroupStudentInDbAsync(await AddGroupInDbAsync(deletedCourseId, _fixture.Create<string>()), studentId);

        var request = new V1SearchStudentCoursesRequest
        {
            StudentId = studentId,
            Filter = new SearchCoursesFilter(),
            PaginationInfo = new PaginationInfo
            {
                Offset = 0,
                PageSize = 10
            }
        };

        // Act
        var response = await SearchStudentCoursesClient.V1SearchStudentCoursesAsync(request);

        // Assert
        response.CourseInfos.Select(courseInfo => courseInfo.Id).Should().BeEquivalentTo([draftCourseId, inProgressCourseId]);
    }

    [Fact]
    public async Task V1SearchStudentCourses_Pagination_ShouldReturnRequestedPage()
    {
        // Arrange
        const int PageSize = 2;
        var studentId = _fixture.Create<long>();
        await AddStudentInDbAsync(studentId);

        var firstCourseId = await AddCourseInDbAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var secondCourseId = await AddCourseInDbAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var thirdCourseId = await AddCourseInDbAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var fourthCourseId = await AddCourseInDbAsync(_fixture.Create<string>(), _fixture.Create<string>());

        await AddGroupStudentInDbAsync(await AddGroupInDbAsync(firstCourseId, _fixture.Create<string>()), studentId);
        await AddGroupStudentInDbAsync(await AddGroupInDbAsync(secondCourseId, _fixture.Create<string>()), studentId);
        await AddGroupStudentInDbAsync(await AddGroupInDbAsync(thirdCourseId, _fixture.Create<string>()), studentId);
        await AddGroupStudentInDbAsync(await AddGroupInDbAsync(fourthCourseId, _fixture.Create<string>()), studentId);

        var request = new V1SearchStudentCoursesRequest
        {
            StudentId = studentId,
            Filter = new SearchCoursesFilter(),
            PaginationInfo = new PaginationInfo
            {
                Offset = 1,
                PageSize = PageSize
            }
        };

        // Act
        var response = await SearchStudentCoursesClient.V1SearchStudentCoursesAsync(request);

        // Assert
        response.CourseInfos.Count.Should().Be(PageSize);
    }

    [Fact]
    public async Task V1SearchStudentCourses_CoursesNotFound_ShouldReturnEmptyCourseInfos()
    {
        // Arrange
        var request = new V1SearchStudentCoursesRequest
        {
            StudentId = _fixture.Create<long>(),
            Filter = new SearchCoursesFilter(),
            PaginationInfo = new PaginationInfo
            {
                Offset = 0,
                PageSize = 10
            }
        };

        // Act
        var response = await SearchStudentCoursesClient.V1SearchStudentCoursesAsync(request);

        // Assert
        response.CourseInfos.Should().BeEmpty();
    }
}
