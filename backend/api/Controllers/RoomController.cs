using System.Text.Json;
using api.DTO.ResponseDTO;
using api.DTO.RoomsDTO;
using api.Helpers;
using api.Helpers.Instances;
using api.Models;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
        public async Task<ActionResult<ResponseDTO<CreatedRoomDTO?, ErrorDTO?>>> UpdateRoom(
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
            [FromQuery(Name = "q")] string? encryptedFilter
        )
        {
            try
            {
                string? decryptedData = _encrypter.DecryptString(encryptedFilter);
                // IRegexController
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new { res = ModelState }
                    );
                }
                FilterParamsDTO filterParams =
                    JsonSerializer.Deserialize<FilterParamsDTO>(decryptedData) ?? new() { };

                DataListPaginationDTO<CreatedRoomDTO?, ErrorDTO?> responseDTO =
                    await _roomService.GetRoomsAsync(filterParams);

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

        // [HttpGet("{id}")]
        [HttpGet("/TestRoom/")]
        public IActionResult GetRoomById()
        // public async Task<ActionResult<ResponseDTO<CreatedRoomDTO?, ErrorDTO?>>> GetRoomById(int id)
        {
            FilterParamsDTO test = new()
            {
                Limit = 10,
                SortOrder = 0,
                Cursor = null,
                SortBy = null,
                Filter = null,
            };
            var Output = _encrypter.EncryptString(JsonSerializer.Serialize(test));
            return Ok(Output);
        }

        [HttpGet("testRoom2")]
        public IActionResult Test()
        {
            _logger.CustomDebug("this is a message send by API Hotels");
            try
            {
                var testObject = new
                {
                    message = "hola",
                    value = 42,
                    isActive = true,
                };
                return Ok(testObject);
            }
            catch (Exception response)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { response });
            }
        }

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
