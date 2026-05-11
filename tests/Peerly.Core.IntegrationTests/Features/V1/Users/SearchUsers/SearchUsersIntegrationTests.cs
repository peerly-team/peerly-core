using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Users.SearchUsers;

public sealed class SearchUsersIntegrationTests : SearchUsersIntegrationTestBase
{
    private const int DefaultLimit = 10;

    private readonly Fixture _fixture = new();

    public SearchUsersIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1SearchUsers_ByName_ShouldReturnMatchingTeachersAndStudents()
    {
        // Arrange
        const string TeacherName = "Алиса Иванова";
        const string StudentName = "Иван Алисов";
        await AddTeacherInDbAsync(_fixture.Create<long>(), name: TeacherName);
        await AddTeacherInDbAsync(_fixture.Create<long>());
        await AddStudentInDbAsync(_fixture.Create<long>(), name: StudentName);
        await AddStudentInDbAsync(_fixture.Create<long>());

        var request = CreateRequest("Алис", [Role.Teacher, Role.Student]);

        // Act
        var response = await SearchUsersClient.V1SearchUsersAsync(request);

        // Assert
        response.Users.Should().HaveCount(2);
        response.Users.Select(u => u.Name).Should().Contain(TeacherName, StudentName);
    }

    [Fact]
    public async Task V1SearchUsers_ByEmailPrefix_ShouldReturnMatchingUsers()
    {
        // Arrange
        const string TeacherEmail = "alice@uni.edu";
        const string StudentEmail = "alice.student@uni.edu";
        await AddTeacherInDbAsync(_fixture.Create<long>(), email: TeacherEmail);
        await AddStudentInDbAsync(_fixture.Create<long>(), email: StudentEmail);
        await AddStudentInDbAsync(_fixture.Create<long>());

        var request = CreateRequest("alice", [Role.Teacher, Role.Student]);

        // Act
        var response = await SearchUsersClient.V1SearchUsersAsync(request);

        // Assert
        response.Users.Select(u => u.Email).Should().BeEquivalentTo(TeacherEmail, StudentEmail);
    }

    [Fact]
    public async Task V1SearchUsers_FilterByTeacherRole_ShouldReturnOnlyTeachers()
    {
        // Arrange
        const string TeacherEmail = "test@uni.edu";
        const string TeacherName = "Test Teacher";
        await AddTeacherInDbAsync(_fixture.Create<long>(), TeacherEmail, TeacherName);
        await AddStudentInDbAsync(_fixture.Create<long>(), "test@student.edu", "Test Student");

        var request = CreateRequest("test", [Role.Teacher]);

        // Act
        var response = await SearchUsersClient.V1SearchUsersAsync(request);

        // Assert
        response.Users.Should().ContainSingle().Which.Should().BeEquivalentTo(
            new
            {
                Name = TeacherName,
                Email = TeacherEmail,
                Role = Role.Teacher
            });
    }

    [Fact]
    public async Task V1SearchUsers_FilterByStudentRole_ShouldReturnOnlyStudents()
    {
        // Arrange
        const string StudentEmail = "test@student.edu";
        await AddTeacherInDbAsync(_fixture.Create<long>(), "test@uni.edu", "Test Teacher");
        await AddStudentInDbAsync(_fixture.Create<long>(), StudentEmail, "Test Student");

        var request = CreateRequest("test", [Role.Student]);

        // Act
        var response = await SearchUsersClient.V1SearchUsersAsync(request);

        // Assert
        response.Users.Should().ContainSingle();
        response.Users[0].Role.Should().Be(Role.Student);
        response.Users[0].Email.Should().Be(StudentEmail);
    }

    [Fact]
    public async Task V1SearchUsers_Limit_ShouldCapTotalResults()
    {
        // Arrange
        const int Limit = 6;
        for (var i = 0; i < Limit; i++)
        {
            await AddTeacherInDbAsync(_fixture.Create<long>(), name: $"Search Teacher {i}");
        }

        for (var i = 0; i < Limit; i++)
        {
            await AddStudentInDbAsync(_fixture.Create<long>(), name: $"Search Student {i}");
        }

        var request = CreateRequest("Search", [Role.Teacher, Role.Student], Limit);

        // Act
        var response = await SearchUsersClient.V1SearchUsersAsync(request);

        // Assert
        response.Users.Should().HaveCount(Limit);
    }

    [Fact]
    public async Task V1SearchUsers_NoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        await AddTeacherInDbAsync(_fixture.Create<long>());

        var request = CreateRequest("zzz", [Role.Teacher, Role.Student]);

        // Act
        var response = await SearchUsersClient.V1SearchUsersAsync(request);

        // Assert
        response.Users.Should().BeEmpty();
    }

    [Fact]
    public async Task V1SearchUsers_SpecialCharactersInQuery_ShouldNotBreakSearch()
    {
        // Arrange
        const string StudentName = "Test%Student";
        await AddTeacherInDbAsync(_fixture.Create<long>());
        await AddStudentInDbAsync(_fixture.Create<long>(), name: StudentName);

        var request = CreateRequest("Test%", [Role.Teacher, Role.Student]);

        // Act
        var response = await SearchUsersClient.V1SearchUsersAsync(request);

        // Assert
        response.Users.Should().NotBeEmpty();
        response.Users.Select(u => u.Name).Should().Contain(StudentName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public async Task V1SearchUsers_QueryTooShort_ShouldReturnInvalidArgument(string query)
    {
        // Arrange
        var request = new V1SearchUsersRequest
        {
            Filter = new V1SearchUsersRequest.Types.SearchUsersFilter { Query = query },
            Limit = DefaultLimit
        };

        // Act
        var act = async () => await SearchUsersClient.V1SearchUsersAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1SearchUsers_InvalidLimit_ShouldReturnInvalidArgument(int limit)
    {
        // Arrange
        var request = new V1SearchUsersRequest
        {
            Filter = new V1SearchUsersRequest.Types.SearchUsersFilter { Query = "test" },
            Limit = limit
        };

        // Act
        var act = async () => await SearchUsersClient.V1SearchUsersAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task V1SearchUsers_UnsupportedRole_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = CreateRequest("test", [Role.Admin]);

        // Act
        var act = async () => await SearchUsersClient.V1SearchUsersAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task V1SearchUsers_EmptyRoles_ShouldReturnEmptyResult()
    {
        // Arrange
        await AddTeacherInDbAsync(_fixture.Create<long>(), name: "Shared Teacher");
        await AddStudentInDbAsync(_fixture.Create<long>(), name: "Shared Student");

        var request = CreateRequest("Shared", []);

        // Act
        var response = await SearchUsersClient.V1SearchUsersAsync(request);

        // Assert
        response.Users.Should().BeEmpty();
    }

    private static V1SearchUsersRequest CreateRequest(
        string query,
        IReadOnlyCollection<Role> roles,
        int limit = DefaultLimit)
    {
        var request = new V1SearchUsersRequest
        {
            Filter = new V1SearchUsersRequest.Types.SearchUsersFilter
            {
                Query = query
            },
            Limit = limit
        };

        request.Filter.Roles.Add(roles);

        return request;
    }
}
