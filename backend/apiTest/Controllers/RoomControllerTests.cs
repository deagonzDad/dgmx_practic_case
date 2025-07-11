using api.Controllers;
using api.DTO.ResponseDTO;
using api.DTO.RoomsDTO;
using api.Helpers.Instances;
using api.Services.Interfaces;
using apiTest.Fixtures;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace apiTest.Controllers;

public class RoomControllerTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly RoomsController _sut;
    private readonly Mock<IRoomService> _roomServiceMock;
    private readonly Mock<ILogger<RoomsController>> _loggerMock;
    private readonly Mock<IEncrypter> _encrypterMock;

    public RoomControllerTests(TestFixture fixture)
    {
        _fixture = fixture;
        _roomServiceMock = _fixture.Freeze<Mock<IRoomService>>();
        _loggerMock = _fixture.Freeze<Mock<ILogger<RoomsController>>>();
        _encrypterMock = _fixture.Freeze<Mock<IEncrypter>>();

        _sut = new RoomsController(
            _roomServiceMock.Object,
            _loggerMock.Object,
            _encrypterMock.Object
        );
    }

    [Fact]
    public async Task GetRoomById_WhenRoomExists_ReturnsOkObjectResultWithData()
    {
        // Arrange
        var roomDto = _fixture.Create<CreatedRoomDTO>();
        var serviceResponse = new ResponseDTO<CreatedRoomDTO?, ErrorDTO?>
        {
            Data = roomDto,
            Message = _fixture.Create<string>(),
            Success = true,
        };
        var roomId = _fixture.Create<int>();

        _roomServiceMock.Setup(s => s.GetRoomByIdAsync(roomId)).ReturnsAsync(serviceResponse);

        // Act
        var actionResult = await _sut.GetRoomById(roomId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Equal(serviceResponse, okResult.Value);
        _roomServiceMock.Verify(s => s.GetRoomByIdAsync(roomId), Times.Once);
    }

    [Fact]
    public async Task GetRoomById_WhenRoomDoesNotExist_ReturnsNotFoundResult()
    {
        // Arrange
        var roomId = _fixture.Create<int>();
        var errorResponse = new ResponseDTO<CreatedRoomDTO?, ErrorDTO?>
        {
            Data = null,
            Message = "Room not found",
            Success = false,
            Code = StatusCodes.Status404NotFound,
        };
        _roomServiceMock.Setup(s => s.GetRoomByIdAsync(roomId)).ReturnsAsync(errorResponse);

        // Act
        var actionResult = await _sut.GetRoomById(roomId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        _roomServiceMock.Verify(s => s.GetRoomByIdAsync(roomId), Times.Once);
    }

    [Fact]
    public async Task GetRooms_WhenCalled_ReturnsOkObjectResultWithPaginatedData()
    {
        // Arrange
        var filterParams = _fixture.Create<FilterParamsDTO>();
        var token = _fixture.Create<string>();
        var decryptedCursor = _fixture.Create<string>();

        var rooms = _fixture.CreateMany<CreatedRoomDTO>(5).ToList();
        var serviceResponse = new DataListPaginationDTO<CreatedRoomDTO?, ErrorDTO?>
        {
            Data = rooms!,
            TotalRecords = rooms.Count,
            Next = "next_cursor",
            Previous = "prev_cursor",
        };

        var encryptedNext = _fixture.Create<string>();
        var encryptedPrev = _fixture.Create<string>();

        _encrypterMock.Setup(e => e.DecryptString(token)).Returns(decryptedCursor);
        _roomServiceMock
            .Setup(s => s.GetRoomsAsync(It.Is<FilterParamsDTO>(f => f.Cursor == decryptedCursor)))
            .ReturnsAsync(serviceResponse);
        _encrypterMock.Setup(e => e.EncryptString("next_cursor")).Returns(encryptedNext);
        _encrypterMock.Setup(e => e.EncryptString("prev_cursor")).Returns(encryptedPrev);

        // Act
        var actionResult = await _sut.GetRooms(filterParams, token);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var resultValue = Assert.IsType<DataListPaginationDTO<CreatedRoomDTO?, ErrorDTO?>>(
            okResult.Value
        );
        Assert.Equal(rooms.Count, resultValue.Data.Count);
        Assert.Equal(encryptedNext, resultValue.Next);
        Assert.Equal(encryptedPrev, resultValue.Previous);

        _roomServiceMock.Verify(s => s.GetRoomsAsync(It.IsAny<FilterParamsDTO>()), Times.Once);
        _encrypterMock.Verify(e => e.DecryptString(token), Times.Once);
        _encrypterMock.Verify(e => e.EncryptString("next_cursor"), Times.Once);
        _encrypterMock.Verify(e => e.EncryptString("prev_cursor"), Times.Once);
    }

    [Fact]
    public async Task CreateRoom_WithValidModel_ReturnsOkResultWithData()
    {
        // Arrange
        var createRoomDto = _fixture.Create<CreateRoomDTO>();
        var createdRoomDto = _fixture.Create<CreatedRoomDTO>();
        var responseDto = new ResponseDTO<CreatedRoomDTO?, ErrorDTO?>
        {
            Data = createdRoomDto,
            Success = true,
            Message = "Room created successfully",
        };

        _roomServiceMock.Setup(s => s.CreateRoomAsync(createRoomDto)).ReturnsAsync(responseDto);

        // Act
        var actionResult = await _sut.CreateRoom(createRoomDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Equal(responseDto, okResult.Value);
        _roomServiceMock.Verify(s => s.CreateRoomAsync(createRoomDto), Times.Once);
    }

    [Fact]
    public async Task CreateRoom_WithInvalidModel_ReturnsInternalServerError()
    {
        // Arrange
        var createRoomDto = _fixture.Create<CreateRoomDTO>();
        _sut.ModelState.AddModelError("Error", "Sample model error");

        // Act
        var actionResult = await _sut.CreateRoom(createRoomDto);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        _roomServiceMock.Verify(s => s.CreateRoomAsync(It.IsAny<CreateRoomDTO>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRoomById_WhenRoomExistsAndModelIsValid_ReturnsOkResult()
    {
        // Arrange
        var roomId = _fixture.Create<int>();
        var updateDto = _fixture.Create<UpdateRoomDTO>();
        var updatedDto = _fixture.Create<CreatedRoomDTO>();
        var responseDto = new ResponseDTO<CreatedRoomDTO?, ErrorDTO?>
        {
            Data = updatedDto,
            Success = true,
            Message = _fixture.Create<string>(),
        };

        _roomServiceMock.Setup(s => s.UpdateRoomAsync(updateDto, roomId)).ReturnsAsync(responseDto);

        // Act
        var actionResult = await _sut.UpdateRoomById(roomId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Equal(responseDto, okResult.Value);
        _roomServiceMock.Verify(s => s.UpdateRoomAsync(updateDto, roomId), Times.Once);
    }

    [Fact]
    public async Task UpdateRoomById_WhenRoomDoesNotExist_ReturnsNotFoundResult()
    {
        // Arrange
        var roomId = _fixture.Create<int>();
        var updateDto = _fixture.Create<UpdateRoomDTO>();
        var responseDto = new ResponseDTO<CreatedRoomDTO?, ErrorDTO?>
        {
            Data = null,
            Success = false,
            Code = StatusCodes.Status404NotFound,
            Message = _fixture.Create<string>(),
        };

        _roomServiceMock.Setup(s => s.UpdateRoomAsync(updateDto, roomId)).ReturnsAsync(responseDto);

        // Act
        var actionResult = await _sut.UpdateRoomById(roomId, updateDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        _roomServiceMock.Verify(s => s.UpdateRoomAsync(updateDto, roomId), Times.Once);
    }

    [Fact]
    public async Task UpdateRoomById_WithInvalidModel_ReturnsInternalServerError()
    {
        // Arrange
        var roomId = _fixture.Create<int>();
        var updateDto = _fixture.Create<UpdateRoomDTO>();
        _sut.ModelState.AddModelError("Error", "Sample model error");

        // Act
        var actionResult = await _sut.UpdateRoomById(roomId, updateDto);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        _roomServiceMock.Verify(
            s => s.UpdateRoomAsync(It.IsAny<UpdateRoomDTO>(), It.IsAny<int>()),
            Times.Never
        );
    }

    [Fact]
    public void GetRoomType_WhenCalled_ReturnsOkResultWithAllRoomTypes()
    {
        // Arrange
        var expectedRoomTypesCount = Enum.GetNames(typeof(api.Models.RoomType)).Length;

        // Act
        var actionResult = _sut.GetRoomType();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var responseValue = Assert.IsType<DataListPaginationDTO<RoomTypeDTO?, ErrorDTO?>>(
            okResult.Value
        );
        Assert.Equal(expectedRoomTypesCount, responseValue.TotalRecords);
        Assert.NotNull(responseValue.Data);
        Assert.Equal(expectedRoomTypesCount, responseValue.Data.Count);
        Assert.Contains(
            responseValue.Data,
            item => item.Name == "Single" && item.Id == (int)api.Models.RoomType.Single
        );
        Assert.Contains(
            responseValue.Data,
            item => item.Name == "Double" && item.Id == (int)api.Models.RoomType.Double
        );
        Assert.Contains(
            responseValue.Data,
            item => item.Name == "Deluxe" && item.Id == (int)api.Models.RoomType.Deluxe
        );
        // Assert.Contains(responseValue.Data, item => item.Name == "Family" && item.Id == (int)api.models.RoomType.Family);
    }
}
