using api.Data;
using api.DTO.ResponseDTO;
using api.DTO.SettingsDTO;
using api.DTO.UsersDTO;
using api.Exceptions;
using api.Helpers;
using api.Helpers.Instances;
using api.Models;
using api.Repository.Interfaces;
using api.Services;
using apiTest.Fixtures;
using AutoFixture;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;

namespace apiTest.Services;

public class AuthServiceTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly AuthService _sut;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly Mock<IHasher> _hasherMock;
    private readonly Mock<JwtTokenGenerator> _jwtGeneratorMock;
    private readonly Mock<AppDbContext> _dbContextMock;
    private readonly Mock<IDbContextTransaction> _transactionMock;
    private readonly Mock<IMapper> _mapperMock;

    public AuthServiceTests(TestFixture fixture)
    {
        _fixture = fixture;
        _userRepoMock = _fixture.Freeze<Mock<IUserRepository>>();
        _roleRepoMock = _fixture.Freeze<Mock<IRoleRepository>>();
        _hasherMock = _fixture.Freeze<Mock<IHasher>>();
        _jwtGeneratorMock = _fixture.Freeze<Mock<JwtTokenGenerator>>();
        _dbContextMock = _fixture.Freeze<Mock<AppDbContext>>();
        _transactionMock = _fixture.Freeze<Mock<IDbContextTransaction>>();
        _mapperMock = _fixture.Freeze<Mock<IMapper>>();

        _dbContextMock
            .Setup(db => db.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transactionMock.Object);

        _transactionMock
            .Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _transactionMock
            .Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = _fixture.Create<AuthService>();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsJwtToken()
    {
        var loginDto = _fixture.Create<UserSignInDTO>();
        var user = _fixture.Create<User>();
        var token = _fixture.Create<JWTTokenResDTO>();

        _userRepoMock
            .Setup(r => r.GetUserByEmailOrUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.VerifyPassword(loginDto.Password, user.Password)).Returns(true);
        _jwtGeneratorMock.Setup(g => g.GenerateToken(user)).Returns(token);

        var result = await _sut.LoginAsync(loginDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(token, result.Data);
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ReturnsError()
    {
        // Arrange
        var loginDto = _fixture.Create<UserSignInDTO>();
        _userRepoMock
            .Setup(r => r.GetUserByEmailOrUsernameAsync(It.IsAny<string>()))
            .ThrowsAsync(new UserNotFoundException(null));

        // Act
        var result = await _sut.LoginAsync(loginDto);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal(StatusCodes.Status400BadRequest, result.Code);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsError()
    {
        // Arrange
        var loginDto = _fixture.Create<UserSignInDTO>();
        var user = _fixture.Create<User>();

        _userRepoMock
            .Setup(r => r.GetUserByEmailOrUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.VerifyPassword(loginDto.Password, user.Password)).Returns(false);

        // Act
        var result = await _sut.LoginAsync(loginDto);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal(StatusCodes.Status400BadRequest, result.Code);
    }

    [Fact]
    public async Task SignupAsync_WithValidData_ReturnsCreatedUser()
    {
        // Arrange
        var createDto = _fixture.Create<UserCreateDTO>();
        var user = _fixture.Create<User>();
        var roles = _fixture.CreateMany<Role>().ToList();
        var createdUserDto = _fixture.Create<UserCreatedDTO>();

        _hasherMock.Setup(h => h.HashPassword(createDto.Password)).Returns("hashed_password");
        _mapperMock.Setup(m => m.Map<User>(createDto)).Returns(user);
        _roleRepoMock.Setup(r => r.GetRolesByIdAsync(createDto.Roles)).ReturnsAsync(roles);
        _userRepoMock.Setup(r => r.CreateUserAsync(user)).Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<UserCreatedDTO>(user)).Returns(createdUserDto);

        // Act
        var result = await _sut.SignupAsync(createDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(createdUserDto, result.Data);
        Assert.Equal(201, result.Code);
        _transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SignupAsync_WhenUserAlreadyExists_ReturnsConflictError()
    {
        // Arrange
        var createDto = _fixture.Create<UserCreateDTO>();
        _userRepoMock
            .Setup(r => r.CreateUserAsync(It.IsAny<User>()))
            .ThrowsAsync(new AlreadyExistException(null));

        // Act
        var result = await _sut.SignupAsync(createDto);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal(StatusCodes.Status409Conflict, result.Code);
        _transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SignupAsync_WhenRoleNotFound_ReturnsBadRequestError()
    {
        // Arrange
        var createDto = _fixture.Create<UserCreateDTO>();
        _roleRepoMock
            .Setup(r => r.GetRolesByIdAsync(It.IsAny<List<int>>()))
            .ThrowsAsync(new RoleNotFoundException(null));

        // Act
        var result = await _sut.SignupAsync(createDto);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal(StatusCodes.Status400BadRequest, result.Code);
        _transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SignupAsync_WhenCommitFails_ThrowsExceptionAndRollsBack()
    {
        // Arrange
        var createDto = _fixture.Create<UserCreateDTO>();
        var user = _fixture.Create<User>();
        var roles = _fixture.CreateMany<Role>().ToList();

        _hasherMock.Setup(h => h.HashPassword(createDto.Password)).Returns("hashed_password");
        _mapperMock.Setup(m => m.Map<User>(createDto)).Returns(user);
        _roleRepoMock.Setup(r => r.GetRolesByIdAsync(createDto.Roles)).ReturnsAsync(roles);
        _userRepoMock.Setup(r => r.CreateUserAsync(user)).Returns(Task.CompletedTask);

        _transactionMock
            .Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Simulated commit failure"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SignupAsync(createDto));
        _transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
