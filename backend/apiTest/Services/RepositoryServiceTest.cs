namespace apiTest.Services;

using api.DTO.ReservationsDTO;
using api.Exceptions;
using api.Models;
using api.Repository.Interfaces;
using api.Services;
using apiTest.Fixtures;
using AutoFixture;
using AutoMapper;
using Moq;

public class RepositoryServiceTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly ReservationService _reservationService;
    private readonly Mock<IReservationRepository> _reservationRepoMock;
    private readonly Mock<IMapper> _mapperMock;

    public RepositoryServiceTests(TestFixture fixture)
    {
        _fixture = fixture;
        _reservationRepoMock = _fixture.Freeze<Mock<IReservationRepository>>();
        _mapperMock = _fixture.Freeze<Mock<IMapper>>();
        _reservationService = _fixture.Create<ReservationService>();
        // _reservationService = _fixture.Create<ReservationService>();
    }

    [Fact]
    public async Task GetReservationAsync_WhenReservationExists_ReturnDTO()
    {
        var reservation = _fixture.Create<Reservation>();
        var reservationDTO = _fixture.Create<CreatedReservationDTO>();
        var expectedDTO = _fixture.Create<CreatedReservationDTO>();
        _reservationRepoMock
            .Setup(repo => repo.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);
        _mapperMock.Setup(map => map.Map<CreatedReservationDTO>(reservation)).Returns(expectedDTO);
        var result = await _reservationService.GetReservationByIdAsync(reservation.Id);
        Assert.NotNull(result.Data);
        Assert.NotEqual(reservationDTO, result.Data);
    }

    [Fact]
    public async Task GetReservationByIdAsync_WhenReservationDoesNotExist_ReturnNull()
    {
        var nonExistedId = _fixture.Create<int>();
        _reservationRepoMock
            .Setup(repo => repo.GetReservationByIdAsync(nonExistedId))
            .ThrowsAsync(new ReservationNotFoundException(null));
        await Assert.ThrowsAsync<ReservationNotFoundException>(
            () => _reservationService.GetReservationByIdAsync(nonExistedId)
        );
    }
}
