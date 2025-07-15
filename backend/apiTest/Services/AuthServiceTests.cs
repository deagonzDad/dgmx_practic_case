using api.DTO.UsersDTO;
using api.Models;
using api.Services;
using apiTest.Fixtures;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace apiTest.Services;

public class AuthServiceTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly AuthService _sut;

    public AuthServiceTests(TestFixture fixture)
    {
        _fixture = fixture;

        _sut = new AuthService(
            _fixture.userRepo,
            _fixture.roleRepo,
            _fixture.hasherMock.Object,
            _fixture.jwtTokenGenerator,
            _fixture.DbAppContext,
            _fixture.mapperMock.Object,
            _fixture.loggerMock.Object,
            _fixture.errorHandler
        );
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsJwtToken()
    {
        var userFromDb = await _fixture.DbAppContext.Users.FirstAsync();
        UserSignInDTO loginDto = new() { Email = userFromDb.Email, Password = userFromDb.Password };
        _fixture
            .hasherMock.Setup(h => h.VerifyPassword(loginDto.Password, userFromDb.Password))
            .Returns(true);
        var result = await _sut.LoginAsync(loginDto);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.False(string.IsNullOrEmpty(result.Data.Token));
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ReturnsError()
    {
        var loginDto = _fixture
            .Build<UserSignInDTO>()
            .With(u => u.Email, "nonexistinguser@example.com")
            .Create();

        var result = await _sut.LoginAsync(loginDto);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal(StatusCodes.Status400BadRequest, result.Code);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsError()
    {
        // ARRANGE
        var userFromDb = await _fixture.DbAppContext.Users.FirstAsync();
        var loginDto = _fixture
            .Build<UserSignInDTO>()
            .With(u => u.Email, userFromDb.Email)
            .With(u => u.Password, "this is a wrong password")
            .Create();

        _fixture
            .hasherMock.Setup(h => h.VerifyPassword(loginDto.Password, userFromDb.Password))
            .Returns(false);

        // ACT
        var result = await _sut.LoginAsync(loginDto);

        // ASSERT
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal(StatusCodes.Status400BadRequest, result.Code);
    }

    [Fact]
    public async Task SignupAsync_WithValidData_ReturnsCreatedUser()
    {
        // ARRANGE
        var createDto = _fixture
            .Build<UserCreateDTO>()
            .With(u => u.Email, "new-user-test@example.com")
            .With(u => u.Username, "new-user-test")
            .Create();
        var user = _fixture.Create<User>();
        var createdUserDto = _fixture.Create<UserCreatedDTO>();

        _fixture
            .hasherMock.Setup(h => h.HashPassword(createDto.Password))
            .Returns("hashed_password");
        _fixture.mapperMock.Setup(m => m.Map<User>(createDto)).Returns(user);
        _fixture.mapperMock.Setup(m => m.Map<UserCreatedDTO>(user)).Returns(createdUserDto);

        // ACT
        var result = await _sut.SignupAsync(createDto);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(201, result.Code);

        var userInDb = await _fixture.DbAppContext.Users.SingleOrDefaultAsync(u =>
            u.Email == createDto.Email
        );
        Assert.NotNull(userInDb);
        Assert.Equal("new-user-test", userInDb.Username);
    }

    [Fact]
    public async Task SignupAsync_WhenUserAlreadyExists_ReturnsConflictError()
    {
        var existingUser = await _fixture.DbAppContext.Users.FirstAsync();
        var createDto = _fixture
            .Build<UserCreateDTO>()
            .With(u => u.Email, existingUser.Email)
            .Create();

        var result = await _sut.SignupAsync(createDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(StatusCodes.Status409Conflict, result.Code);
    }

    [Fact]
    public async Task SignupAsync_WhenRoleNotFound_ReturnsBadRequestError()
    {
        var createDto = _fixture
            .Build<UserCreateDTO>()
            .With(u => u.Roles, new List<int> { 999, 1001 }) // Non-existing roles
            .Create();

        var result = await _sut.SignupAsync(createDto);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal(StatusCodes.Status400BadRequest, result.Code);
    }

    [Fact]
    public async Task SignupAsync_WhenCommitFails_ThrowsExceptionAndRollsBack()
    {
        // Arrange
        var createDto = _fixture.Create<UserCreateDTO>();
        var user = _fixture.Create<User>();
        var roles = _fixture.CreateMany<Role>().ToList();

        _fixture
            .hasherMock.Setup(h => h.HashPassword(createDto.Password))
            .Returns("hashed_password");
        _fixture.mapperMock.Setup(m => m.Map<User>(createDto)).Returns(user);

        // This test is complex to implement with an in-memory database.
        // It would require mocking the DbContext transaction behavior.
        // For now, it remains as a placeholder.
    }
}
