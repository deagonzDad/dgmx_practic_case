namespace apiTest.Services;

using api.DTO.ReservationsDTO;
using api.Models;
using api.Repository.Interfaces;
using api.Services;
using AutoFixture;
using AutoMapper;
using Moq;

public class RepositoryServiceTests(
    Mock<IReservationRepository> reservationRepoMock,
    Mock<IRoomRepository> roomRepoMock,
    Mock<IPaymentRepository> paymentRepoMock,
    Mock<IMapper> mapperMock,
    ReservationService reservationService,
    IFixture fixture
)
{
    private readonly Mock<IReservationRepository> _reservationRepoMock = reservationRepoMock;
    private readonly Mock<IRoomRepository> _roomRepoMock = roomRepoMock;
    private readonly Mock<IPaymentRepository> _paymentRepoMock = paymentRepoMock;
    private readonly Mock<IMapper> _mapperMock = mapperMock;
    private readonly ReservationService _reservationService = reservationService;
    private readonly IFixture _fixture = fixture;

    [Fact]
    public async Task CreateReservationAsync_WhenRoomIsAvailable()
    {
        var createDTO = _fixture.Create<CreateReservationDTO>();
        var userId = _fixture.Create<int>();
        var availableRoom = _fixture
            .Build<Room>()
            .With(r => r.IsAvailable, true)
            .With(r => r.Id, createDTO.RoomId)
            .Create();
        var reservationEntity = _fixture.Create<Reservation>();
        // var responseDTO = _fixture.Create<Reservation
    }
}
