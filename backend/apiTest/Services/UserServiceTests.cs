using api.DTO.ResponseDTO;
using api.DTO.UsersDTO;
using api.Exceptions;
using api.Models;
using api.Repository.Interfaces;
using api.Services;
using apiTest.Fixtures;
using AutoFixture;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Moq;

namespace apiTest.Services;

public class UserServiceTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly UserService _sut;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IMapper> _mapperMock;

    public UserServiceTests(TestFixture fixture)
    {
        _fixture = fixture;
        _userRepoMock = _fixture.Freeze<Mock<IUserRepository>>();
        _mapperMock = _fixture.Freeze<Mock<IMapper>>();
        _sut = _fixture.Create<UserService>();
    }

    [Fact]
    public async Task GetUsersAsync_WhenUsersExist_ReturnsPaginatedData()
    {
        // Arrange
        var filterParams = _fixture.Create<FilterParamsDTO>();
        var users = _fixture.CreateMany<User>(5).ToList();
        var userDtos = _fixture.CreateMany<UserCreatedDTO>(5).ToList();
        var nextCursor = _fixture.Create<int>();
        var totalCount = users.Count;

        _userRepoMock
            .Setup(r => r.GetUsersAsync(filterParams))
            .ReturnsAsync((users, nextCursor, totalCount));
        _mapperMock.Setup(m => m.Map<List<UserCreatedDTO>>(users)).Returns(userDtos);

        // Act
        var result = await _sut.GetUsersAsync(filterParams);

        // Assert
        Assert.Equal(userDtos, result.Data!);
        Assert.Equal(totalCount, result.TotalRecords);
        Assert.Equal(nextCursor.ToString(), result.Next);
    }

    [Fact]
    public async Task GetUsersAsync_WhenNoUsersExist_ReturnsEmptyData()
    {
        // Arrange
        var filterParams = _fixture.Create<FilterParamsDTO>();
        var users = new List<User>();
        var userDtos = new List<UserCreatedDTO>();
        int? nextCursor = null;
        var totalCount = 0;

        _userRepoMock
            .Setup(r => r.GetUsersAsync(filterParams))
            .ReturnsAsync((users, nextCursor, totalCount));
        _mapperMock.Setup(m => m.Map<List<UserCreatedDTO>>(users)).Returns(userDtos);

        // Act
        var result = await _sut.GetUsersAsync(filterParams);

        // Assert
        Assert.Empty(result.Data);
        Assert.Equal(0, result.TotalRecords);
    }

    [Fact]
    public async Task GetUsersAsync_WhenRepositoryThrowsException_ReturnsError()
    {
        // Arrange
        var filterParams = _fixture.Create<FilterParamsDTO>();
        _userRepoMock
            .Setup(r => r.GetUsersAsync(filterParams))
            .ThrowsAsync(new UserNotFoundException(null));

        // Act
        var result = await _sut.GetUsersAsync(filterParams);

        // Assert
        Assert.Null(result.Data);
        // Assert.Equal(StatusCodes.Status404NotFound, result.Code);
    }
}
