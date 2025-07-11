using api.Controllers;
using api.DTO.ResponseDTO;
using api.DTO.SettingsDTO;
using api.DTO.UsersDTO;
using api.Services.Interfaces;
using apiTest.Fixtures;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace apiTest.Controllers;

public class AuthControllerTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly AuthController _sut;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;

    public AuthControllerTests(TestFixture fixture)
    {
        _fixture = fixture;
        _authServiceMock = _fixture.Freeze<Mock<IAuthService>>();
        _loggerMock = _fixture.Freeze<Mock<ILogger<AuthController>>>();

        _sut = new AuthController(_authServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkResultWithToken()
    {
        var loginDto = _fixture.Create<UserSignInDTO>();
        var tokenDto = _fixture.Create<JWTTokenResDTO>();
        ResponseDTO<JWTTokenResDTO?, ErrorDTO?> responseDto = new()
        {
            Data = tokenDto,
            Success = true,
            Message = _fixture.Create<string>(),
        };

        _authServiceMock.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync(responseDto);

        var actionResult = await _sut.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Equal(responseDto, okResult.Value);
        _authServiceMock.Verify(s => s.LoginAsync(loginDto), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorizedResult()
    {
        var loginDto = _fixture.Create<UserSignInDTO>();
        ResponseDTO<JWTTokenResDTO?, ErrorDTO?> responseDto = new()
        {
            Data = null,
            Success = false,
            Code = StatusCodes.Status401Unauthorized,
            Message = _fixture.Create<string>(),
        };

        _authServiceMock.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync(responseDto);

        var actionResult = await _sut.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
        _authServiceMock.Verify(s => s.LoginAsync(loginDto), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidModel_ReturnsInternalServerError()
    {
        var loginDto = _fixture.Create<UserSignInDTO>();
        _sut.ModelState.AddModelError("Error", "Sample model error");

        var actionResult = await _sut.Login(loginDto);

        var statusCodeResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusCodeResult.StatusCode);
        _authServiceMock.Verify(s => s.LoginAsync(It.IsAny<UserSignInDTO>()), Times.Never);
    }

    [Fact]
    public async Task SignUp_WithValidData_ReturnsOkResultWithCreatedUser()
    {
        var userCreateDto = _fixture.Create<UserCreateDTO>();
        var userCreatedDto = _fixture.Create<UserCreatedDTO>();
        var responseDto = new ResponseDTO<UserCreatedDTO?, ErrorDTO?>
        {
            Data = userCreatedDto,
            Success = true,
            Message = _fixture.Create<string>(),
        };

        _authServiceMock.Setup(s => s.SignupAsync(userCreateDto)).ReturnsAsync(responseDto);

        var actionResult = await _sut.SignUp(userCreateDto);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Equal(responseDto, okResult.Value);
        _authServiceMock.Verify(s => s.SignupAsync(userCreateDto), Times.Once);
    }

    [Fact]
    public async Task SignUp_WhenUserAlreadyExists_ReturnsConflictResult()
    {
        var userCreateDto = _fixture.Create<UserCreateDTO>();
        var responseDto = new ResponseDTO<UserCreatedDTO?, ErrorDTO?>
        {
            Data = null,
            Success = false,
            Code = StatusCodes.Status409Conflict,
            Message = _fixture.Create<string>(),
        };

        _authServiceMock.Setup(s => s.SignupAsync(userCreateDto)).ReturnsAsync(responseDto);

        // Act
        var actionResult = await _sut.SignUp(userCreateDto);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status409Conflict, conflictResult.StatusCode);
        _authServiceMock.Verify(s => s.SignupAsync(userCreateDto), Times.Once);
    }

    [Fact]
    public async Task SignUp_WithInvalidModel_ReturnsInternalServerError()
    {
        var userCreateDto = _fixture.Create<UserCreateDTO>();
        _sut.ModelState.AddModelError("Error", "Sample model error");

        var actionResult = await _sut.SignUp(userCreateDto);

        // Assert
        var statusCodeResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusCodeResult.StatusCode);
        _authServiceMock.Verify(s => s.SignupAsync(It.IsAny<UserCreateDTO>()), Times.Never);
    }
}
