using api.Data;
using api.DTO.ResponseDTO;
using api.DTO.RoomsDTO;
using api.Exceptions;
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

public class RoomServiceTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly RoomService _sut;
    private readonly Mock<IRoomRepository> _roomRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<AppDbContext> _dbContextMock;
    private readonly Mock<IDbContextTransaction> _transactionMock;

    public RoomServiceTests(TestFixture fixture)
    {
        _fixture = fixture;
        _roomRepoMock = _fixture.Freeze<Mock<IRoomRepository>>();
        _mapperMock = _fixture.Freeze<Mock<IMapper>>();
        _dbContextMock = _fixture.Freeze<Mock<AppDbContext>>();
        _transactionMock = _fixture.Freeze<Mock<IDbContextTransaction>>();

        _dbContextMock
            .Setup(db => db.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transactionMock.Object);

        _sut = _fixture.Create<RoomService>();
    }

    [Fact]
    public async Task CreateRoomAsync_WhenSuccessful_ReturnsCreatedRoom()
    {
        // Arrange
        var createDto = _fixture.Create<CreateRoomDTO>();
        var roomMdl = _fixture.Create<Room>();
        var createdDto = _fixture.Create<CreatedRoomDTO>();

        _mapperMock.Setup(m => m.Map<Room>(createDto)).Returns(roomMdl);
        _roomRepoMock.Setup(r => r.CreateRoomAsync(roomMdl)).ReturnsAsync((true, roomMdl));
        _roomRepoMock.Setup(r => r.SetRoomActivation(roomMdl, true)).Returns(roomMdl);
        _roomRepoMock.Setup(r => r.UpdateRoomAsync(roomMdl, roomMdl.Id)).ReturnsAsync(roomMdl);
        _mapperMock.Setup(m => m.Map<CreatedRoomDTO>(roomMdl)).Returns(createdDto);

        // Act
        var result = await _sut.CreateRoomAsync(createDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(createdDto, result.Data);
        _transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetRoomByIdAsync_WhenRoomExists_ReturnsRoom()
    {
        // Arrange
        var roomId = _fixture.Create<int>();
        var room = _fixture.Create<Room>();
        var createdDto = _fixture.Create<CreatedRoomDTO>();

        _roomRepoMock.Setup(r => r.GetRoomByIdAsync(roomId)).ReturnsAsync(room);
        _mapperMock.Setup(m => m.Map<CreatedRoomDTO>(room)).Returns(createdDto);

        // Act
        var result = await _sut.GetRoomByIdAsync(roomId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(createdDto, result.Data);
    }

    [Fact]
    public async Task GetRoomByIdAsync_WhenRoomDoesNotExist_ReturnsError()
    {
        // Arrange
        var roomId = _fixture.Create<int>();
        _roomRepoMock
            .Setup(r => r.GetRoomByIdAsync(roomId))
            .ThrowsAsync(new RoomNotFoundException(null));

        // Act
        var result = await _sut.GetRoomByIdAsync(roomId);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal(StatusCodes.Status404NotFound, result.Code);
    }

    [Fact]
    public async Task UpdateRoomAsync_WhenRoomExists_ReturnsUpdatedRoom()
    {
        // Arrange
        var roomId = _fixture.Create<int>();
        var updateDto = _fixture.Create<UpdateRoomDTO>();
        var roomObj = _fixture.Create<Room>();
        var updatedRoom = _fixture.Create<Room>();
        var createdDto = _fixture.Create<CreatedRoomDTO>();

        _mapperMock.Setup(m => m.Map<Room>(updateDto)).Returns(roomObj);
        _roomRepoMock.Setup(r => r.UpdateRoomAsync(roomObj, roomId)).ReturnsAsync(updatedRoom);
        _mapperMock.Setup(m => m.Map<CreatedRoomDTO>(updatedRoom)).Returns(createdDto);

        // Act
        var result = await _sut.UpdateRoomAsync(updateDto, roomId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(createdDto, result.Data);
    }

    [Fact]
    public async Task DeleteRoomAsync_WhenRoomExists_ReturnsSuccess()
    {
        // Arrange
        var roomId = _fixture.Create<int>();
        var room = _fixture.Create<Room>();
        _roomRepoMock.Setup(r => r.GetRoomByIdAsync(roomId)).ReturnsAsync(room);
        _roomRepoMock.Setup(r => r.LogicDeleteRoomAsync(room)).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteRoomAsync(roomId);

        // Assert
        Assert.True(result.Success);
        _transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRoomAsync_WhenRoomDoesNotExist_ReturnsError()
    {
        // Arrange
        var roomId = _fixture.Create<int>();
        _roomRepoMock
            .Setup(r => r.GetRoomByIdAsync(roomId))
            .ThrowsAsync(new RoomNotFoundException(null));

        // Act
        var result = await _sut.DeleteRoomAsync(roomId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(StatusCodes.Status404NotFound, result.Code);
        _transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
