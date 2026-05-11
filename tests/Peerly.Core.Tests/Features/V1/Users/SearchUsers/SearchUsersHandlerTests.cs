using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Users.SearchUsers;
using Peerly.Core.Models.Users;
using Xunit;

namespace Peerly.Core.Tests.Features.V1.Users.SearchUsers;

public sealed class SearchUsersHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();

    private readonly Fixture _fixture = new();
    private readonly SearchUsersHandler _handler;

    public SearchUsersHandlerTests()
    {
        SetupUnitOfWorkFactory();
        _handler = new SearchUsersHandler(_unitOfWorkFactoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_FilterWithBothRoles_ShouldCallSearchAndReturnResults()
    {
        // Arrange
        var filter = _fixture.Build<UserFilter>()
            .With(filter => filter.Roles, [UserRole.Teacher, UserRole.Student])
            .Create();
        var query = _fixture.Build<SearchUsersQuery>()
            .With(query => query.Filter, filter)
            .Create();

        var expectedResults = new List<User>
        {
            _fixture.Build<User>().With(user => user.Role, UserRole.Teacher).Create(),
            _fixture.Build<User>().With(user => user.Role, UserRole.Student).Create()
        };

        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyUserSearchRepository.ListAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        response.Users.Should().BeEquivalentTo(expectedResults);
        _unitOfWorkMock.Verify(
            uow => uow.ReadOnlyUserSearchRepository.ListAsync(filter, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NoResults_ShouldReturnEmptyCollection()
    {
        // Arrange
        var filter = _fixture.Build<UserFilter>()
            .With(filter => filter.Roles, [])
            .Create();
        var query = _fixture.Build<SearchUsersQuery>()
            .With(query => query.Filter, filter)
            .Create();

        _unitOfWorkMock
            .Setup(uow => uow.ReadOnlyUserSearchRepository.ListAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var response = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        response.Users.Should().BeEmpty();
    }

    private void SetupUnitOfWorkFactory()
    {
        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);
    }
}
