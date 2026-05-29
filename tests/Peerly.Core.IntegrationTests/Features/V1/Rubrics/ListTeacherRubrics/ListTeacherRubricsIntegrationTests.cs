using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Core.IntegrationTests.Features.V1.Rubrics.ListTeacherRubrics.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;
using Peerly.Core.V1;
using Xunit;

namespace Peerly.Core.IntegrationTests.Features.V1.Rubrics.ListTeacherRubrics;

public sealed class ListTeacherRubricsIntegrationTests : RubricIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public ListTeacherRubricsIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    private ListTeacherRubricsGrpcClient ListTeacherRubricsClient => Fixture.ListTeacherRubricsClient;

    [Fact]
    public async Task V1ListTeacherRubrics_TeacherHasRubrics_ShouldReturnRubrics()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        await AddTeacherInDbAsync(teacherId);

        await AddRubricInDbAsync(teacherId, "Rubric A");
        await AddRubricInDbAsync(teacherId, "Rubric B");

        var request = new V1ListTeacherRubricsRequest { TeacherId = teacherId };

        // Act
        var response = await ListTeacherRubricsClient.V1ListTeacherRubricsAsync(request);

        // Assert
        response.Rubrics.Should().HaveCount(2);
    }

    [Fact]
    public async Task V1ListTeacherRubrics_TeacherHasNoRubrics_ShouldReturnEmpty()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        await AddTeacherInDbAsync(teacherId);

        var request = new V1ListTeacherRubricsRequest { TeacherId = teacherId };

        // Act
        var response = await ListTeacherRubricsClient.V1ListTeacherRubricsAsync(request);

        // Assert
        response.Rubrics.Should().BeEmpty();
    }

    [Fact]
    public async Task V1ListTeacherRubrics_ShouldNotReturnOtherTeachersRubrics()
    {
        // Arrange
        var teacherId = _fixture.Create<long>();
        var otherTeacherId = teacherId + 1;
        await AddTeacherInDbAsync(teacherId);
        await AddTeacherInDbAsync(otherTeacherId);

        await AddRubricInDbAsync(teacherId, "My Rubric");
        await AddRubricInDbAsync(otherTeacherId, "Other Rubric");

        var request = new V1ListTeacherRubricsRequest { TeacherId = teacherId };

        // Act
        var response = await ListTeacherRubricsClient.V1ListTeacherRubricsAsync(request);

        // Assert
        response.Rubrics.Should().ContainSingle();
        response.Rubrics[0].Name.Should().Be("My Rubric");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task V1ListTeacherRubrics_NotPositiveTeacherId_ShouldReturnInvalidArgument(long teacherId)
    {
        // Arrange
        var request = new V1ListTeacherRubricsRequest { TeacherId = teacherId };

        // Act
        var act = async () => await ListTeacherRubricsClient.V1ListTeacherRubricsAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.TeacherId));
    }
}
