using api.DTO.ResponseDTO;
using api.DTO.RoomsDTO;
using api.Helpers;
using api.Models;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class Rooms(IRoomService roomService, ILogger<Rooms> logger) : MyBaseController
    {
        private readonly ILogger<Rooms> _logger = logger;
        private readonly IRoomService _roomService = roomService;

        [HttpPost]
        public async Task<ActionResult<ResponseDTO<CreatedRoomDTO?, ErrorDTO?>>> CreateRoom(
            [FromBody] CreateRoomDTO room
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
                ResponseDTO<CreatedRoomDTO?, ErrorDTO?> responseDTO =
                    await _roomService.CreateRoomAsync(room);

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
            ActionResult<ResponseDTO<DataListPaginationDTO<CreatedRoomDTO?>?, ErrorDTO?>>
        > GetRooms(int limit, string? cursor, string? sortBy, string? sortOrder, string? filter)
        {
            try
            {
                // IRegexController
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new { res = ModelState }
                    );
                }
                ResponseDTO<DataListPaginationDTO<CreatedRoomDTO?>?, ErrorDTO?> responseDTO =
                    await _roomService.GetRoomsAsync(limit, cursor, sortBy, sortOrder, filter);

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
        // [HttpGet("{id}")]
        // public async Task<ActionResult<ResponseDTO<CreatedRoomDTO?, ErrorDTO?>>> GetRoomById(int id)
        // {
        //     return Ok();
        // }
        // try
        // {
        // [HttpGet]
        // public IActionResult Test()
        // {
        //     _logger.CustomDebug("this is a message send by API Hotels");
        //     try
        //     {
        //         var testObject = new
        //         {
        //             message = "hola",
        //             value = 42,
        //             isActive = true,
        //         };
        //         return Ok(testObject);
        //     }
        //     catch (Exception response)
        //     {
        //         return StatusCode(StatusCodes.Status500InternalServerError, new { response });
        //     }
        // }
        // }
    }
}
