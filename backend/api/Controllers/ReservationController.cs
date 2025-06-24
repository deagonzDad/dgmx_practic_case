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
        ILogger<Reservation> logger,
        IEncrypter encrypter
    ) : MyBaseController
    {
        private readonly ILogger<Reservation> _logger = logger;
        private readonly IReservationService _reservationService = reservationService;
        private readonly IEncrypter _encrypter = encrypter;

        [HttpPost]
        public async Task<
            ActionResult<ResponseDTO<CreatedReservationDTO?, ErrorDTO?>>
        > CreateReservation([FromBody] CreateReservationDTO reservation)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new { res = ModelState }
                    );
                }
                ResponseDTO<CreatedReservationDTO?, ErrorDTO?> responseDTO =
                    await _reservationService.CreateReservationAsync(reservation);
                return CreateResponse(responseDTO);
            }
            catch (Exception ex)
            {
                string messageError = "Something goes wrong unexpectedly";
                _logger.LogError(ex, "{messageError}", messageError);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { res = ModelState }
                );
            }
        }

        [HttpGet]
        public async Task<
            ActionResult<DataListPaginationDTO<CreatedReservationDTO?, ErrorDTO?>>
        > GetReservations(
            [FromQuery] FilterParamsDTO encryptedFilter,
            [FromQuery(Name = "token")] string? token
        )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new { res = ModelState }
                    );
                }
                string decrypted = _encrypter.DecryptString(token);
                encryptedFilter.Cursor = string.IsNullOrEmpty(decrypted) ? null : decrypted;
                DataListPaginationDTO<CreatedReservationDTO?, ErrorDTO?> responseDTO =
                    await _reservationService.GetReservationsAsync(encryptedFilter);
                responseDTO.Next = string.IsNullOrEmpty(responseDTO.Next)
                    ? null
                    : _encrypter.EncryptString(responseDTO.Next);
                responseDTO.Previous = string.IsNullOrEmpty(responseDTO.Previous)
                    ? null
                    : _encrypter.EncryptString(responseDTO.Previous);
                return CreateListResponse(responseDTO);
            }
            catch (Exception ex)
            {
                string messageError = "Something goes wrong unexpectedly";
                _logger.LogError(ex, "{messageError}", messageError);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { res = ModelState }
                );
            }
        }

        [HttpGet("/IdReservation")]
        public async Task<
            ActionResult<ResponseDTO<CreatedReservationDTO?, ErrorDTO?>>
        > GetReservationById(int IdReservation)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new { res = ModelState }
                    );
                }
                ResponseDTO<CreatedReservationDTO?, ErrorDTO?> responseDTO =
                    await _reservationService.GetReservationByIdAsync(IdReservation);
                return CreateResponse(responseDTO);
            }
            catch (Exception ex)
            {
                string messageError = "Something goes wrong unexpectedly";
                _logger.LogError(ex, "{messageError}", messageError);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { res = ModelState }
                );
            }
        }
    }
}
