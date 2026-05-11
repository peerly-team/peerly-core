using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;
using CourseStatusModel = Peerly.Core.Models.Courses.CourseStatus;
using ProtoCourseStatus = Peerly.Core.V1.CourseStatus;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.SearchTeacherCourses;

public sealed class SearchTeacherCoursesIntegrationTests : SearchTeacherCoursesIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public SearchTeacherCoursesIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1SearchTeacherCourses_TeacherHasCourses_ShouldReturnFilteredNotDeletedCourses()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var courseTeacherId = _fixture.Create<long>();
        var otherTeacherId = _fixture.Create<long>();
        var teacherEmail = _fixture.Create<string>();
        var teacherName = _fixture.Create<string>();
        var courseTeacherEmail = _fixture.Create<string>();
        var courseTeacherName = _fixture.Create<string>();
        await AddTeacherInDbAsync(teacherId, teacherEmail, teacherName);
        await AddTeacherInDbAsync(courseTeacherId, courseTeacherEmail, courseTeacherName);
        await AddTeacherInDbAsync(otherTeacherId);

        var inProgressCourseName = _fixture.Create<string>();
        var inProgressCourseDescription = _fixture.Create<string>();
        var inProgressCourseId = await AddCourseInDbAsync(inProgressCourseName, inProgressCourseDescription, CourseStatusModel.InProgress);

        var finishedCourseId = await AddCourseInDbAsync(_fixture.Create<string>(), _fixture.Create<string>(), CourseStatusModel.Finished);
        var deletedCourseId = await AddCourseInDbAsync(_fixture.Create<string>(), _fixture.Create<string>(), CourseStatusModel.Deleted);
        var otherTeacherCourseId = await AddCourseInDbAsync(_fixture.Create<string>(), _fixture.Create<string>(), CourseStatusModel.Finished);

        await AddCourseTeacherInDbAsync(inProgressCourseId, teacherId);
        await AddCourseTeacherInDbAsync(inProgressCourseId, courseTeacherId);
        await AddGroupTeacherInDbAsync(await AddGroupInDbAsync(finishedCourseId, _fixture.Create<string>()), teacherId);
        await AddCourseTeacherInDbAsync(deletedCourseId, teacherId);
        await AddCourseTeacherInDbAsync(otherTeacherCourseId, otherTeacherId);

        var request = new V1SearchTeacherCoursesRequest
        {
            TeacherId = teacherId,
            Filter = new SearchCoursesFilter
            {
                CourseStatuses = { ProtoCourseStatus.InProgress }
            },
            PaginationInfo = new PaginationInfo
            {
                Offset = 0,
                PageSize = 10
            }
        };

        // Act
        var response = await SearchTeacherCoursesClient.V1SearchTeacherCoursesAsync(request);

        // Assert
        var courseInfo = response.CourseInfos.Should().ContainSingle().Which;
        courseInfo.Id.Should().Be(inProgressCourseId);
        courseInfo.Name.Should().Be(inProgressCourseName);
        courseInfo.Description.Should().Be(inProgressCourseDescription);
        courseInfo.Status.Should().Be(ProtoCourseStatus.InProgress);
        courseInfo.Teachers
            .Select(teacherInfo => new { teacherInfo.TeacherId, teacherInfo.Email, teacherInfo.Name })
            .Should()
            .BeEquivalentTo(
            [
                new { TeacherId = teacherId, Email = teacherEmail, Name = teacherName },
                new { TeacherId = courseTeacherId, Email = courseTeacherEmail, Name = courseTeacherName }
            ]);
    }

    [Fact]
    public async Task V1SearchTeacherCourses_EmptyFilter_ShouldReturnAllNotDeletedTeacherCourses()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var teacherEmail = _fixture.Create<string>();
        var teacherName = _fixture.Create<string>();
        await AddTeacherInDbAsync(teacherId, teacherEmail, teacherName);

        var draftCourseId = await AddCourseInDbAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var inProgressCourseId = await AddCourseInDbAsync(_fixture.Create<string>(), _fixture.Create<string>(), CourseStatusModel.InProgress);
        var deletedCourseId = await AddCourseInDbAsync(_fixture.Create<string>(), _fixture.Create<string>(), CourseStatusModel.Deleted);

        await AddCourseTeacherInDbAsync(draftCourseId, teacherId);
        await AddGroupTeacherInDbAsync(await AddGroupInDbAsync(inProgressCourseId, _fixture.Create<string>()), teacherId);
        await AddCourseTeacherInDbAsync(deletedCourseId, teacherId);

        var request = new V1SearchTeacherCoursesRequest
        {
            TeacherId = teacherId,
            Filter = new SearchCoursesFilter(),
            PaginationInfo = new PaginationInfo
            {
                Offset = 0,
                PageSize = 10
            }
        };

        // Act
        var response = await SearchTeacherCoursesClient.V1SearchTeacherCoursesAsync(request);

        // Assert
        response.CourseInfos.Select(courseInfo => courseInfo.Id).Should().BeEquivalentTo([draftCourseId, inProgressCourseId]);
        var draftCourseInfo = response.CourseInfos.Single(courseInfo => courseInfo.Id == draftCourseId);
        draftCourseInfo.Teachers
            .Select(teacherInfo => new { teacherInfo.TeacherId, teacherInfo.Email, teacherInfo.Name })
            .Should()
            .BeEquivalentTo([new { TeacherId = teacherId, Email = teacherEmail, Name = teacherName }]);

        var inProgressCourseInfo = response.CourseInfos.Single(courseInfo => courseInfo.Id == inProgressCourseId);
        inProgressCourseInfo.Teachers.Should().BeEmpty();
    }

    [Fact]
    public async Task V1SearchTeacherCourses_Pagination_ShouldReturnRequestedPage()
    {
        // Arrange
        const int PageSize = 2;
        var teacherId = _fixture.Create<long>();
        await AddTeacherInDbAsync(teacherId);

        var firstCourseId = await AddCourseInDbAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var secondCourseId = await AddCourseInDbAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var thirdCourseId = await AddCourseInDbAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var fourthCourseId = await AddCourseInDbAsync(_fixture.Create<string>(), _fixture.Create<string>());

        await AddCourseTeacherInDbAsync(firstCourseId, teacherId);
        await AddCourseTeacherInDbAsync(secondCourseId, teacherId);
        await AddCourseTeacherInDbAsync(thirdCourseId, teacherId);
        await AddCourseTeacherInDbAsync(fourthCourseId, teacherId);

        var request = new V1SearchTeacherCoursesRequest
        {
            TeacherId = teacherId,
            Filter = new SearchCoursesFilter(),
            PaginationInfo = new PaginationInfo
            {
                Offset = 1,
                PageSize = PageSize
            }
        };

        // Act
        var response = await SearchTeacherCoursesClient.V1SearchTeacherCoursesAsync(request);

        // Assert
        response.CourseInfos.Count.Should().Be(PageSize);
    }

    [Fact]
    public async Task V1SearchTeacherCourses_CoursesNotFound_ShouldReturnEmptyCourseInfos()
    {
        // Arrange
        var request = new V1SearchTeacherCoursesRequest
        {
            TeacherId = _fixture.Create<long>(),
            Filter = new SearchCoursesFilter(),
            PaginationInfo = new PaginationInfo
            {
                Offset = 0,
                PageSize = 10
            }
        };

        // Act
        var response = await SearchTeacherCoursesClient.V1SearchTeacherCoursesAsync(request);

        // Assert
        response.CourseInfos.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1SearchTeacherCourses_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = new V1SearchTeacherCoursesRequest
        {
            TeacherId = teacherId,
            Filter = new SearchCoursesFilter(),
            PaginationInfo = new PaginationInfo
            {
                Offset = 0,
                PageSize = 10
            }
        };

        // Act
        var act = async () => await SearchTeacherCoursesClient.V1SearchTeacherCoursesAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }

    [Fact]
    public async Task V1SearchTeacherCourses_NegativeOffset_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = new V1SearchTeacherCoursesRequest
        {
            TeacherId = _fixture.Create<long>(),
            Filter = new SearchCoursesFilter(),
            PaginationInfo = new PaginationInfo
            {
                Offset = -1,
                PageSize = 10
            }
        };

        // Act
        var act = async () => await SearchTeacherCoursesClient.V1SearchTeacherCoursesAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.PaginationInfo.Offset));
    }

    [Fact]
    public async Task V1SearchTeacherCourses_NegativePageSize_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = new V1SearchTeacherCoursesRequest
        {
            TeacherId = _fixture.Create<long>(),
            Filter = new SearchCoursesFilter(),
            PaginationInfo = new PaginationInfo
            {
                Offset = 0,
                PageSize = -1
            }
        };

        // Act
        var act = async () => await SearchTeacherCoursesClient.V1SearchTeacherCoursesAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.PaginationInfo.PageSize));
    }
}
