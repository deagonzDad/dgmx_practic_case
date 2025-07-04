using api.Controllers;
using api.DTO.ReservationsDTO;
using api.DTO.ResponseDTO;
using api.Exceptions;
using api.Helpers.Instances;
using api.Models;
using api.Services.Interfaces;
using apiTest.Fixtures;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace apiTest.Controllers;

public class ReservationsControllerTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly ReservationController _sut;
    private readonly Mock<IReservationService> _reservationServiceMock;
    private readonly Mock<ILogger<ReservationController>> _loggerMock;
    private readonly Mock<IEncrypter> _encrypterMock;

    public ReservationsControllerTests(TestFixture fixture)
    {
        _fixture = fixture;
        _reservationServiceMock = _fixture.Freeze<Mock<IReservationService>>();
        _loggerMock = _fixture.Freeze<Mock<ILogger<ReservationController>>>();
        _encrypterMock = _fixture.Freeze<Mock<IEncrypter>>();

        _sut = new ReservationController(
            _reservationServiceMock.Object,
            _loggerMock.Object,
            _encrypterMock.Object
        );
    }

    [Fact]
    public async Task GetById_WhenReservationExists_ReturnsOkObjectResultWithData()
    {
        var reservationDto = _fixture.Create<CreatedReservationDTO>();
        var serviceResponse = new ResponseDTO<CreatedReservationDTO?, ErrorDTO?>
        {
            Data = reservationDto,
            Message = _fixture.Create<string>(),
            Success = true,
        };
        var reservationId = _fixture.Create<int>();

        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(reservationId))
            .ReturnsAsync(serviceResponse);

        var actionResult = await _sut.GetReservationById(reservationId);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(serviceResponse, okResult.Value);
    }

    [Fact]
    public async Task GetById_WhenReservationDoesNotExist_ReturnsNotFoundResult()
    {
        var reservationId = _fixture.Create<int>();
        var errorResponse = new ResponseDTO<CreatedReservationDTO?, ErrorDTO?>
        {
            // Data = null,
            Message = "Reservation not found",
            Success = false,
            Code = StatusCodes.Status404NotFound,
        };
        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(reservationId))
            .ReturnsAsync(errorResponse);

        var actionResult = await _sut.GetReservationById(reservationId);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }
}
