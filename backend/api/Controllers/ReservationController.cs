using api.DTO.ReservationsDTO;
using api.DTO.ResponseDTO;
using api.Helpers;
using api.Helpers.Instances;
using api.Models;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReservationController(
        IReservationService reservationService,
        ILogger<ReservationController> logger,
        IEncrypter encrypter
    ) : MyBaseController
    {
        private readonly ILogger<ReservationController> _logger = logger;
        private readonly IReservationService _reservationService = reservationService;
        private readonly IEncrypter _encrypter = encrypter;

        [HttpPost]
        public async Task<
            ActionResult<ResponseDTO<CreatedReservationListDTO?, ErrorDTO?>>
        > CreateReservation([FromBody] CreateReservationDTO reservation)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            ResponseDTO<CreatedReservationListDTO?, ErrorDTO?> responseDTO =
                await _reservationService.CreateReservationAsync(reservation);
            return CreateResponse(responseDTO);
        }

        [HttpGet]
        public async Task<
            ActionResult<DataListPaginationDTO<CreatedReservationListDTO?, ErrorDTO?>>
        > GetReservations(
            [FromQuery] FilterParamsDTO encryptedFilter,
            [FromQuery(Name = "token")] string? token
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            string decrypted = _encrypter.DecryptString(token);
            encryptedFilter.Cursor = string.IsNullOrEmpty(decrypted) ? null : decrypted;
            DataListPaginationDTO<CreatedReservationListDTO?, ErrorDTO?> responseDTO =
                await _reservationService.GetReservationsAsync(encryptedFilter);
            responseDTO.Next = string.IsNullOrEmpty(responseDTO.Next)
                ? null
                : _encrypter.EncryptString(responseDTO.Next);
            responseDTO.Previous = string.IsNullOrEmpty(responseDTO.Previous)
                ? null
                : _encrypter.EncryptString(responseDTO.Previous);
            return CreateListResponse(responseDTO);
        }

        [HttpGet("{IdRes}")]
        public async Task<
            ActionResult<ResponseDTO<CreatedReservationDTO?, ErrorDTO?>>
        > GetReservationById([FromRoute(Name = "IdRes")] int IdReservation)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            ResponseDTO<CreatedReservationDTO?, ErrorDTO?> responseDTO =
                await _reservationService.GetReservationByIdAsync(IdReservation);
            return CreateResponse(responseDTO);
        }
    }
}
