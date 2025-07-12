using api.Data;
using api.DTO.ReservationsDTO;
using api.DTO.ResponseDTO;
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

public class ReservationServiceTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly ReservationService _sut;
    private readonly Mock<IReservationRepository> _reservationRepoMock;
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly Mock<IRoomRepository> _roomRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<AppDbContext> _dbContextMock;
    private readonly Mock<IDbContextTransaction> _transactionMock;

    public ReservationServiceTests(TestFixture fixture)
    {
        _fixture = fixture;
        _reservationRepoMock = _fixture.Freeze<Mock<IReservationRepository>>();
        _paymentRepoMock = _fixture.Freeze<Mock<IPaymentRepository>>();
        _roomRepoMock = _fixture.Freeze<Mock<IRoomRepository>>();
        _mapperMock = _fixture.Freeze<Mock<IMapper>>();
        _dbContextMock = _fixture.Freeze<Mock<AppDbContext>>();
        _transactionMock = _fixture.Freeze<Mock<IDbContextTransaction>>();

        _dbContextMock
            .Setup(db => db.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transactionMock.Object);

        _sut = _fixture.Create<ReservationService>();
    }

    [Fact]
    public async Task CreateReservationAsync_WhenSuccessful_ReturnsCreatedReservation()
    {
        // Arrange
        var createDto = _fixture.Create<CreateReservationDTO>();
        var roomPrice = _fixture.Create<decimal>();
        var payment = _fixture.Create<Payment>();
        var paymentCreated = _fixture.Create<Payment>();
        var reservationMdl = _fixture.Create<Reservation>();
        var createdDto = _fixture.Create<CreatedReservationListDTO>();

        _roomRepoMock.Setup(r => r.GetPriceByIdAsync(createDto.RoomId)).ReturnsAsync(roomPrice);
        _mapperMock.Setup(m => m.Map<Payment>(createDto)).Returns(payment);
        _paymentRepoMock
            .Setup(p => p.CreatePaymentAsync(payment))
            .ReturnsAsync((true, paymentCreated));
        _mapperMock.Setup(m => m.Map<Reservation>(createDto)).Returns(reservationMdl);
        _reservationRepoMock
            .Setup(r => r.CreateReservationAsync(reservationMdl))
            .Returns(Task.CompletedTask);
        _mapperMock
            .Setup(m => m.Map<CreatedReservationListDTO>(reservationMdl))
            .Returns(createdDto);

        // Act
        var result = await _sut.CreateReservationAsync(createDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(createdDto, result.Data);
        _transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenDbUpdateFails_ReturnsErrorResponse()
    {
        // Arrange
        var createDto = _fixture.Create<CreateReservationDTO>();
        _roomRepoMock
            .Setup(r => r.GetPriceByIdAsync(createDto.RoomId))
            .ThrowsAsync(new UpdateException(null, "DB error"));

        // Act
        var result = await _sut.CreateReservationAsync(createDto);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal(StatusCodes.Status400BadRequest, result.Code);
        _transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetReservationByIdAsync_WhenReservationExists_ReturnsReservation()
    {
        // Arrange
        var reservationId = _fixture.Create<int>();
        var reservation = _fixture.Create<Reservation>();
        var createdDto = _fixture.Create<CreatedReservationDTO>();

        _reservationRepoMock
            .Setup(r => r.GetReservationByIdAsync(reservationId))
            .ReturnsAsync(reservation);
        _mapperMock.Setup(m => m.Map<CreatedReservationDTO>(reservation)).Returns(createdDto);

        // Act
        var result = await _sut.GetReservationByIdAsync(reservationId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(createdDto, result.Data);
    }

    [Fact]
    public async Task GetReservationByIdAsync_WhenReservationDoesNotExist_ReturnsErrorResponse()
    {
        // Arrange
        var reservationId = _fixture.Create<int>();
        _reservationRepoMock
            .Setup(r => r.GetReservationByIdAsync(reservationId))
            .ThrowsAsync(new ReservationNotFoundException(null));

        // Act
        var result = await _sut.GetReservationByIdAsync(reservationId);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal(StatusCodes.Status404NotFound, result.Code);
    }

    [Fact]
    public async Task GetReservationsAsync_WhenCalled_ReturnsPaginatedData()
    {
        // Arrange
        var filterParams = _fixture.Create<FilterParamsDTO>();
        var dataList = _fixture.CreateMany<CreatedReservationListDTO>(5).ToList();
        var nextCursor = _fixture.Create<int>();
        var totalCount = _fixture.Create<int>();

        _reservationRepoMock
            .Setup(r => r.GetReservations(filterParams))
            .ReturnsAsync((dataList, nextCursor, totalCount));

        // Act
        var result = await _sut.GetReservationsAsync(filterParams);

        // Assert
        Assert.Equal(dataList, result.Data!);
        Assert.Equal(totalCount, result.TotalRecords);
        Assert.Equal(nextCursor.ToString(), result.Next);
    }
}
