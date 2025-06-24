using api.DTO.ResponseDTO;
using api.DTO.RoomsDTO;
using api.Helpers;
using api.Helpers.Instances;
using api.Models;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class Rooms(IRoomService roomService, ILogger<Rooms> logger, IEncrypter encrypter)
        : MyBaseController
    {
        private readonly ILogger<Rooms> _logger = logger;
        private readonly IRoomService _roomService = roomService;
        private readonly IEncrypter _encrypter = encrypter;

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

        [HttpPut("/{IdRoom}")]
        public async Task<ActionResult<ResponseDTO<CreatedRoomDTO?, ErrorDTO?>>> UpdateRoomById(
            int IdRoom,
            [FromBody] UpdateRoomDTO room
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
                    await _roomService.UpdateRoomAsync(room, IdRoom);
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
        public async Task<ActionResult<DataListPaginationDTO<CreatedRoomDTO?, ErrorDTO?>>> GetRooms(
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
                DataListPaginationDTO<CreatedRoomDTO?, ErrorDTO?> responseDTO =
                    await _roomService.GetRoomsAsync(encryptedFilter);
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

        [HttpGet("/{IdRoom}")]
        public async Task<ActionResult<ResponseDTO<CreatedRoomDTO?, ErrorDTO?>>> GetRoomById(
            int IdRoom
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
                    await _roomService.GetRoomByIdAsync(IdRoom);
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

        // [HttpGet("/TestRoomFilter/")]
        // public IActionResult GetRoomById()
        // {
        //     FilterParamsDTO initialFilter = new()
        //     {
        //         Limit = 10,
        //         SortOrder = 0,
        //         Cursor = null,
        //         SortBy = null,
        //         Filter = null,
        //     };
        //     string Output = _encrypter.EncryptString(JsonSerializer.Serialize(initialFilter));
        //     return Ok(Output);
        // }

        // [HttpGet("testRoom2")]
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

        [HttpGet]
        [Route("/getRoomType")]
        public ActionResult<DataListPaginationDTO<RoomTypeDTO?, ErrorDTO?>> GetRoomType()
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
                var enumList = new List<RoomTypeDTO?>();
                foreach (RoomType type in RoomType.GetValues(typeof(RoomType)))
                {
                    enumList.Add(new RoomTypeDTO { Name = type.ToString(), Id = (int)type });
                }
                DataListPaginationDTO<RoomTypeDTO?, ErrorDTO?> responseDTO = new()
                {
                    Data = enumList,
                    TotalRecords = enumList.Count,
                    Next = null,
                    Previous = null,
                };
                return CreateListResponse(responseDTO);
            }
            catch (Exception ex)
            {
                string MessageError = "Something goes wrong unexpectedly";
                _logger.LogError(ex, "{MessageError}", MessageError);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { res = ModelState }
                );
            }
        }
    }
}
