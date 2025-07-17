using api.DTO.ResponseDTO;
using api.DTO.UsersDTO;
using api.Exceptions;
using api.Models;
using api.Repository.Interfaces;
using api.Services;
using apiTest.Fixtures;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace apiTest.Services;

public class UserServiceTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly UserService _sut;

    public UserServiceTests(TestFixture fixture)
    {
        _fixture = fixture;
        _sut = new UserService(
            // _fixture.DbAppContext,
            _fixture.mapperMock.Object,
            // _fixture.Create<ILogger<UserService>>(),
            // _fixture.errorHandler,
            _fixture.userRepo
        );
    }

    [Fact]
    public async Task GetUsersAsync_WhenCalled_ReturnsPaginatedData()
    {
        // Arrange
        var filterParams = new FilterParamsDTO { Limit = 5 };
        var allUsersInDb = await _fixture
            .DbAppContext.Users.Where(u => u.IsActive == true)
            .OrderBy(u => u.Id)
            .ToListAsync();
        var expectedTotalCount = allUsersInDb.Count;
        var expectedPageOfUsers = allUsersInDb.Take(filterParams.Limit).ToList();
        string? expectedNextCursor =
            (expectedPageOfUsers.Count > filterParams.Limit)
                ? expectedPageOfUsers.LastOrDefault()?.Id.ToString()
                : null;
        List<User>? capturedListData = null;
        var createdDto = _fixture
            .Build<UserCreatedDTO>()
            .CreateMany(expectedPageOfUsers.Count)
            .ToList();

        _fixture
            .mapperMock.Setup(m => m.Map<List<UserCreatedDTO>>(It.IsAny<List<User>>()))
            .Callback(
                (object inputList) =>
                {
                    capturedListData = inputList as List<User>;
                }
            )
            .Returns(createdDto);

        // Act
        var result = await _sut.GetUsersAsync(filterParams);

        // Assert
        Assert.Equal(expectedTotalCount, result.TotalRecords);
        Assert.Equal(expectedNextCursor, string.IsNullOrEmpty(result.Next) ? null : result.Next);
        Assert.NotNull(result.Data);
        Assert.Null(result.Error);
        Assert.Equal(filterParams.Limit, result.Data.Count);

        Assert.NotNull(capturedListData);
        var outputIds = capturedListData.Select(r => r.Id);
        var expectedIds = expectedPageOfUsers.Select(r => r.Id);
        Assert.Equal(expectedIds, outputIds);
    }

    [Fact]
    public async Task GetUsersAsync_WhenNoUsersExist_ReturnsEmptyData()
    {
        // Arrange
        var filterParams = _fixture.Create<FilterParamsDTO>();
        var userRepoMock = new Mock<IUserRepository>();
        var users = new List<User>();
        int? nextCursor = null;
        var totalCount = 0;

        userRepoMock
            .Setup(r => r.GetUsersAsync(filterParams))
            .ReturnsAsync((users, nextCursor, totalCount));
        _fixture.mapperMock.Setup(m => m.Map<List<UserCreatedDTO>>(users)).Returns([]);

        var sut = new UserService(
            // _fixture.DbAppContext,
            _fixture.mapperMock.Object,
            // _fixture.Create<ILogger<UserService>>(),
            // _fixture.errorHandler,
            userRepoMock.Object
        );

        // Act
        var result = await sut.GetUsersAsync(filterParams);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.TotalRecords);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task GetUsersAsync_WhenRepositoryThrowsException_ReturnsError()
    {
        // Arrange
        var filterParams = _fixture.Create<FilterParamsDTO>();
        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock
            .Setup(r => r.GetUsersAsync(filterParams))
            .ThrowsAsync(new UserNotFoundException(null));

        var sut = new UserService(_fixture.mapperMock.Object, userRepoMock.Object);
        await Assert.ThrowsAsync<UserNotFoundException>(async () =>
            await sut.GetUsersAsync(filterParams)
        );
    }
}
