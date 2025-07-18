using api.DTO.ResponseDTO;
using api.DTO.RoomsDTO;
using api.Exceptions;
using api.Models;
using api.Services;
using apiTest.Fixtures;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace apiTest.Services;

public class RoomServiceTests
{
    private readonly TestFixture _fixture;
    private readonly RoomService _sut;

    public RoomServiceTests()
    {
        _fixture = new TestFixture();
        _sut = new RoomService(
            _fixture.DbAppContext,
            _fixture.roomRepo,
            _fixture.mapperMock.Object
        );
    }

    [Fact]
    public async Task CreateRoomAsync_WhenSuccessful_ReturnsCreatedRoom()
    {
        var createDto = _fixture.Create<CreateRoomDTO>();
        var roomMdl = _fixture
            .Build<Room>()
            .With(r => r.Id, 0)
            .With(r => r.IsActive, true)
            .With(r => r.IsAvailable, true)
            .Without(r => r.Reservations)
            .Create();
        var createdDto = _fixture.Create<CreatedRoomDTO>();

        _fixture.mapperMock.Setup(m => m.Map<Room>(createDto)).Returns(roomMdl);
        _fixture.mapperMock.Setup(m => m.Map<CreatedRoomDTO>(roomMdl)).Returns(createdDto);

        var result = await _sut.CreateRoomAsync(createDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(createdDto, result.Data);
    }

    [Fact]
    public async Task GetRoomByIdAsync_WhenRoomExists_ReturnsRoom()
    {
        var room = await _fixture.DbAppContext.Rooms.FirstAsync();
        var createdDto = _fixture.Create<CreatedRoomDTO>();

        _fixture.mapperMock.Setup(m => m.Map<CreatedRoomDTO>(room)).Returns(createdDto);

        var result = await _sut.GetRoomByIdAsync(room.Id);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(createdDto, result.Data);
    }

    [Fact]
    public async Task GetRoomByIdAsync_WhenRoomDoesNotExist_ReturnsError()
    {
        var roomId = 999;

        await Assert.ThrowsAsync<RoomNotFoundException>(async () =>
            await _sut.GetRoomByIdAsync(roomId)
        );
    }

    [Fact]
    public async Task UpdateRoomAsync_WhenRoomExists_ReturnsUpdatedRoom()
    {
        int roomId = await _fixture.DbAppContext.Rooms.Select(r => r.Id).FirstAsync();
        var updateDto = _fixture.Create<UpdateRoomDTO>();
        var updatedRoom = _fixture.Build<Room>().Create();
        var createdDto = _fixture.Create<CreatedRoomDTO>();

        _fixture.mapperMock.Setup(m => m.Map<Room>(updateDto)).Returns(updatedRoom);
        _fixture.mapperMock.Setup(m => m.Map<CreatedRoomDTO>(updatedRoom)).Returns(createdDto);

        var result = await _sut.UpdateRoomAsync(updateDto, roomId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        var updatedRoomFromDb = await _fixture.DbAppContext.Rooms.FirstAsync();

        Assert.NotNull(updatedRoomFromDb);
        Assert.Equal(updatedRoom.RoomNumber, updatedRoomFromDb.RoomNumber);
        Assert.Equal(updatedRoom.RoomType, updatedRoomFromDb.RoomType);
        Assert.Equal(updatedRoom.PricePerNight, updatedRoomFromDb.PricePerNight);
        Assert.Equal(updatedRoom.IsAvailable, updatedRoomFromDb.IsAvailable);
    }

    [Fact]
    public async Task DeleteRoomAsync_WhenRoomExists_ReturnsSuccess()
    {
        var room = await _fixture.DbAppContext.Rooms.FirstAsync();

        var result = await _sut.DeleteRoomAsync(room.Id);

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task DeleteRoomAsync_WhenRoomDoesNotExist_ReturnsError()
    {
        var roomId = 999;

        await Assert.ThrowsAsync<RoomNotFoundException>(async () =>
            await _sut.DeleteRoomAsync(roomId)
        );
    }

    [Fact] //TODO: review the test having error in the Asserts
    public async Task GetRoomsAsync_WhenCalled_ReturnsPaginatedData()
    {
        var filterParams = new FilterParamsDTO { Limit = 5 };
        List<Room> allRoomInDb = await _fixture
            .DbAppContext.Rooms.Where(r => r.IsActive == true)
            .OrderBy(r => r.Id)
            .ToListAsync();
        var expectedTotalCount = allRoomInDb.Count;
        List<Room> expectedPageOfRoom = [.. allRoomInDb.Take(filterParams.Limit)];
        string? expectedNextCursor = expectedPageOfRoom.LastOrDefault()?.Id.ToString();

        List<Room>? capturedListData = [];
        var createdDto = _fixture
            .Build<CreatedRoomDTO>()
            .CreateMany(expectedPageOfRoom.Count)
            .ToList();

        _fixture
            .mapperMock.Setup(m => m.Map<List<CreatedRoomDTO>>(It.IsAny<List<Room>>()))
            .Callback(
                (object inputList) =>
                {
                    capturedListData = inputList as List<Room>;
                }
            )
            .Returns(createdDto);

        var result = await _sut.GetRoomsAsync(filterParams);

        // Assert
        Assert.Equal(expectedTotalCount, result.TotalRecords);
        Assert.Equal(expectedNextCursor, string.IsNullOrEmpty(result.Next) ? null : result.Next);
        Assert.NotNull(result.Data);
        Assert.Null(result.Error);
        // Assert.Equal(filterParams.Limit, result.Data.Count);//uncomment this line if the test have more than the limit of elements

        var outputIds = capturedListData.Select(r => r.Id);
        var expectedIds = expectedPageOfRoom.Select(r => r.Id);
        Assert.Equal(outputIds, expectedIds);
    }
}
