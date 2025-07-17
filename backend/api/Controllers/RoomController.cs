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
    public class RoomsController(
        IRoomService roomService,
        ILogger<RoomsController> logger,
        IEncrypter encrypter
    ) : MyBaseController
    {
        private readonly ILogger<RoomsController> _logger = logger;
        private readonly IRoomService _roomService = roomService;
        private readonly IEncrypter _encrypter = encrypter;

        [HttpPost]
        public async Task<ActionResult<ResponseDTO<CreatedRoomDTO?, ErrorDTO?>>> CreateRoom(
            [FromBody] CreateRoomDTO room
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ResponseDTO<CreatedRoomDTO?, ErrorDTO?> responseDTO =
                await _roomService.CreateRoomAsync(room);

            return CreateResponse(responseDTO);
        }

        [HttpPut("{IdRoom}")]
        public async Task<ActionResult<ResponseDTO<CreatedRoomDTO?, ErrorDTO?>>> UpdateRoomById(
            int IdRoom,
            [FromBody] UpdateRoomDTO room
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ResponseDTO<CreatedRoomDTO?, ErrorDTO?> responseDTO =
                await _roomService.UpdateRoomAsync(room, IdRoom);
            return CreateResponse(responseDTO);
        }

        [HttpGet]
        public async Task<ActionResult<DataListPaginationDTO<CreatedRoomDTO?, ErrorDTO?>>> GetRooms(
            [FromQuery] FilterParamsDTO encryptedFilter,
            [FromQuery(Name = "token")] string? token
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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

        [HttpGet("{IdRoom}")]
        public async Task<ActionResult<ResponseDTO<CreatedRoomDTO?, ErrorDTO?>>> GetRoomById(
            int IdRoom
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ResponseDTO<CreatedRoomDTO?, ErrorDTO?> responseDTO =
                await _roomService.GetRoomByIdAsync(IdRoom);
            return CreateResponse(responseDTO);
        }

        [HttpGet]
        [Route("getRoomType")]
        public ActionResult<DataListPaginationDTO<RoomTypeDTO?, ErrorDTO?>> GetRoomType()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
    }
}
