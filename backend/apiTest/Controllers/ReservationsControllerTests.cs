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

public class ReservationsControllerTests
{
    private readonly TestFixture _fixture;
    private readonly ReservationController _sut;
    private readonly Mock<IReservationService> _reservationServiceMock;
    private readonly Mock<ILogger<ReservationController>> _loggerMock;
    private readonly Mock<IEncrypter> _encrypterMock;

    public ReservationsControllerTests()
    {
        _fixture = new TestFixture();
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
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Equal(serviceResponse, okResult.Value);
    }

    [Fact]
    public async Task GetById_WhenReservationDoesNotExist_ReturnsNotFoundResult()
    {
        var reservationId = _fixture.Create<int>();
        var errorResponse = new ResponseDTO<CreatedReservationDTO?, ErrorDTO?>
        {
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

    [Fact]
    public async Task CreateReservation_WithValidModel_ReturnsOkResult()
    {
        var createDto = _fixture.Create<CreateReservationDTO>();
        var createdDto = _fixture.Create<CreatedReservationListDTO>();
        var responseDto = new ResponseDTO<CreatedReservationListDTO?, ErrorDTO?>
        {
            Data = createdDto,
            Success = true,
            Message = _fixture.Create<string>(),
        };

        _reservationServiceMock
            .Setup(s => s.CreateReservationAsync(createDto))
            .ReturnsAsync(responseDto);

        var actionResult = await _sut.CreateReservation(createDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Equal(responseDto, okResult.Value);
        _reservationServiceMock.Verify(s => s.CreateReservationAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task CreateReservation_WithInvalidModel_ReturnsInternalServerError()
    {
        var createDto = _fixture.Create<CreateReservationDTO>();
        _sut.ModelState.AddModelError("Error", "Sample model error");

        var actionResult = await _sut.CreateReservation(createDto);

        // Assert
        var statusCodeResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusCodeResult.StatusCode);
        _reservationServiceMock.Verify(
            s => s.CreateReservationAsync(It.IsAny<CreateReservationDTO>()),
            Times.Never
        );
    }

    [Fact]
    public async Task GetReservations_WhenCalled_ReturnsOkObjectResultWithPaginatedData()
    {
        var filterParams = _fixture.Create<FilterParamsDTO>();
        var token = _fixture.Create<string>();
        var decryptedCursor = _fixture.Create<string>();

        var reservations = _fixture.CreateMany<CreatedReservationListDTO>(5).ToList();
        var serviceResponse = new DataListPaginationDTO<CreatedReservationListDTO?, ErrorDTO?>
        {
            Data = reservations!,
            TotalRecords = reservations.Count,
            Next = "next_cursor",
            Previous = "prev_cursor",
        };

        var encryptedNext = _fixture.Create<string>();
        var encryptedPrev = _fixture.Create<string>();

        _encrypterMock.Setup(e => e.DecryptString(token)).Returns(decryptedCursor);
        _reservationServiceMock
            .Setup(s =>
                s.GetReservationsAsync(It.Is<FilterParamsDTO>(f => f.Cursor == decryptedCursor))
            )
            .ReturnsAsync(serviceResponse);
        _encrypterMock.Setup(e => e.EncryptString("next_cursor")).Returns(encryptedNext);
        _encrypterMock.Setup(e => e.EncryptString("prev_cursor")).Returns(encryptedPrev);

        // Act
        var actionResult = await _sut.GetReservations(filterParams, token);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var resultValue = Assert.IsType<
            DataListPaginationDTO<CreatedReservationListDTO?, ErrorDTO?>
        >(okResult.Value);
        Assert.Equal(reservations.Count, resultValue.Data.Count);
        Assert.Equal(encryptedNext, resultValue.Next);
        Assert.Equal(encryptedPrev, resultValue.Previous);

        _reservationServiceMock.Verify(
            s => s.GetReservationsAsync(It.IsAny<FilterParamsDTO>()),
            Times.Once
        );
        _encrypterMock.Verify(e => e.DecryptString(token), Times.Once);
        _encrypterMock.Verify(e => e.EncryptString("next_cursor"), Times.Once);
        _encrypterMock.Verify(e => e.EncryptString("prev_cursor"), Times.Once);
    }
}
