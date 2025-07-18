using api.DTO.ReservationsDTO;
using api.DTO.ResponseDTO;
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

public class ReservationServiceTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly ReservationService _sut;

    public ReservationServiceTests(TestFixture fixture)
    {
        _fixture = fixture;
        _sut = new ReservationService(
            _fixture.DbAppContext,
            _fixture.reservationRepo,
            _fixture.paymentRepo,
            _fixture.roomRepo,
            _fixture.mapperMock.Object
        // _fixture.errorHandler,
        // _fixture.Create<ILogger<ReservationService>>()
        );
    }

    [Fact]
    public async Task CreateReservationAsync_WhenSuccessful_ReturnsCreatedReservation()
    {
        var room = await _fixture.DbAppContext.Rooms.FirstAsync();
        var user = await _fixture.DbAppContext.Users.FirstAsync();
        var createDto = _fixture
            .Build<CreateReservationDTO>()
            .With(r => r.RoomId, room.Id)
            .With(r => r.ClientId, user.Id)
            .Create();

        var payment = _fixture
            .Build<Payment>()
            .With(p => p.AmountPerNight, room.PricePerNight)
            .Without(p => p.Reservation)
            .Without(p => p.ReservationId)
            .With(p => p.Id)
            .Create();

        var reservationMdl = _fixture
            .Build<Reservation>()
            .With(r => r.Payment, payment)
            .With(r => r.RoomId, room.Id)
            .With(r => r.UserId, user.Id)
            .Without(r => r.Payment)
            .Without(r => r.PaymentId)
            .Without(r => r.User)
            .Without(r => r.Room)
            .Without(r => r.Payment)
            .With(r => r.Id, 0)
            .Create();

        var createdDto = _fixture.Create<CreatedReservationListDTO>();

        _fixture.mapperMock.Setup(m => m.Map<Payment>(createDto)).Returns(payment);
        _fixture.mapperMock.Setup(m => m.Map<Reservation>(createDto)).Returns(reservationMdl);
        _fixture
            .mapperMock.Setup(m => m.Map<CreatedReservationListDTO>(reservationMdl))
            .Returns(createdDto);

        // Act
        var result = await _sut.CreateReservationAsync(createDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(createdDto, result.Data);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenRoomDoesNotExist_ReturnsErrorResponse()
    {
        var room = await _fixture.DbAppContext.Rooms.FirstAsync();
        var user = await _fixture.DbAppContext.Users.FirstAsync();
        var createDto = _fixture
            .Build<CreateReservationDTO>()
            .With(r => r.ClientId, user.Id)
            .With(r => r.RoomId, 999)
            .Create();
        var payment = _fixture
            .Build<Payment>()
            .With(p => p.AmountPerNight, room.PricePerNight)
            .Without(p => p.Reservation)
            .Without(p => p.ReservationId)
            .With(p => p.Id)
            .Create();
        var reservationMdl = _fixture
            .Build<Reservation>()
            .With(r => r.Payment, payment)
            .With(r => r.RoomId, 999)
            .With(r => r.UserId, user.Id)
            .Without(r => r.Payment)
            .Without(r => r.PaymentId)
            .Without(r => r.User)
            .Without(r => r.Room)
            .Without(r => r.Payment)
            .With(r => r.Id, 0)
            .Create();
        await Assert.ThrowsAsync<RoomNotFoundException>(async () =>
            await _sut.CreateReservationAsync(createDto)
        );
    }

    [Fact]
    public async Task GetReservationByIdAsync_WhenReservationExists_ReturnsReservation()
    {
        var reservation = await _fixture.DbAppContext.Reservations.FirstAsync();
        var createdDto = _fixture.Create<CreatedReservationDTO>();

        _fixture
            .mapperMock.Setup(m => m.Map<CreatedReservationDTO>(reservation))
            .Returns(createdDto);

        var result = await _sut.GetReservationByIdAsync(reservation.Id);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(createdDto, result.Data);
    }

    [Fact]
    public async Task GetReservationByIdAsync_WhenReservationDoesNotExist_ReturnsErrorResponse()
    {
        var reservationId = 999;
        await Assert.ThrowsAsync<ReservationNotFoundException>(async () =>
            await _sut.GetReservationByIdAsync(reservationId)
        );
    }

    [Fact]
    public async Task GetReservationsAsync_WhenCalled_ReturnsPaginatedData()
    {
        FilterParamsDTO filterParams = new() { Limit = 5 };
        List<Reservation> allReservationsInDb = await _fixture
            .DbAppContext.Reservations.OrderBy(r => r.Id)
            .ToListAsync();
        var expectedTotalCount = allReservationsInDb.Count;
        List<Reservation> expectedPageOfReservations =
        [
            .. allReservationsInDb.Take(filterParams.Limit),
        ];
        string expectedNextCursor = expectedPageOfReservations.Last().Id.ToString();

        var result = await _sut.GetReservationsAsync(filterParams);

        // Assert
        Assert.Equal(expectedTotalCount, result.TotalRecords);
        Assert.Equal(expectedNextCursor, result.Next!.ToString());
        Assert.Null(result.Error);
        Assert.NotNull(result.Data);
        Assert.Equal(filterParams.Limit, result.Data.Count);
        var expectedIds = expectedPageOfReservations.Select(r => r.Id);
        var actualIds = result.Data.Select(r => r!.ReservationId);
        Assert.Equal(expectedIds, actualIds);
    }
}
