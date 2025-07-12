using api.Controllers;
using api.DTO.ResponseDTO;
using api.DTO.UsersDTO;
using api.Helpers.Instances;
using api.Services.Interfaces;
using apiTest.Fixtures;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace apiTest.Controllers;

public class UserControllerTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly UserController _sut;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ILogger<UserController>> _loggerMock;
    private readonly Mock<IEncrypter> _encrypterMock;

    public UserControllerTests(TestFixture fixture)
    {
        _fixture = fixture;
        _userServiceMock = _fixture.Freeze<Mock<IUserService>>();
        _loggerMock = _fixture.Freeze<Mock<ILogger<UserController>>>();
        _encrypterMock = _fixture.Freeze<Mock<IEncrypter>>();

        _sut = new UserController(
            _userServiceMock.Object,
            _loggerMock.Object,
            _encrypterMock.Object
        );
    }

    [Fact]
    public async Task GetUsersAsync_WhenCalled_ReturnsOkObjectResultWithPaginatedData()
    {
        // Arrange
        var filterParams = _fixture.Create<FilterParamsDTO>();
        var token = _fixture.Create<string>();
        var decryptedCursor = _fixture.Create<string>();

        var users = _fixture.CreateMany<UserCreatedDTO>(5).ToList();
        var serviceResponse = new DataListPaginationDTO<UserCreatedDTO?, ErrorDTO?>
        {
            Data = users!,
            TotalRecords = users.Count,
            Next = "next_cursor",
            Previous = "prev_cursor",
        };

        var encryptedNext = _fixture.Create<string>();
        var encryptedPrev = _fixture.Create<string>();

        _encrypterMock.Setup(e => e.DecryptString(token)).Returns(decryptedCursor);
        _userServiceMock
            .Setup(s => s.GetUsersAsync(It.Is<FilterParamsDTO>(f => f.Cursor == decryptedCursor)))
            .ReturnsAsync(serviceResponse);
        _encrypterMock.Setup(e => e.EncryptString("next_cursor")).Returns(encryptedNext);
        _encrypterMock.Setup(e => e.EncryptString("prev_cursor")).Returns(encryptedPrev);

        // Act
        var actionResult = await _sut.GetUsersAsync(filterParams, token);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var resultValue = Assert.IsType<DataListPaginationDTO<UserCreatedDTO?, ErrorDTO?>>(
            okResult.Value
        );
        Assert.Equal(users.Count, resultValue.Data.Count);
        Assert.Equal(encryptedNext, resultValue.Next);
        Assert.Equal(encryptedPrev, resultValue.Previous);

        _userServiceMock.Verify(s => s.GetUsersAsync(It.IsAny<FilterParamsDTO>()), Times.Once);
        _encrypterMock.Verify(e => e.DecryptString(token), Times.Once);
        _encrypterMock.Verify(e => e.EncryptString("next_cursor"), Times.Once);
        _encrypterMock.Verify(e => e.EncryptString("prev_cursor"), Times.Once);
    }

    [Fact]
    public async Task GetUsersAsync_WithInvalidModel_ReturnsInternalServerError()
    {
        // Arrange
        var filterParams = _fixture.Create<FilterParamsDTO>();
        var token = _fixture.Create<string>();
        _sut.ModelState.AddModelError("Error", "Sample model error");

        // Act
        var actionResult = await _sut.GetUsersAsync(filterParams, token);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        _userServiceMock.Verify(s => s.GetUsersAsync(It.IsAny<FilterParamsDTO>()), Times.Never);
    }
}
