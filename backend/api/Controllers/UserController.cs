using api.DTO.ResponseDTO;
using api.DTO.UsersDTO;
using api.Helpers;
using api.Helpers.Instances;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController(
        IUserService userService,
        ILogger<UserController> logger,
        IEncrypter encrypter
    ) : MyBaseController
    {
        private readonly ILogger<UserController> _logger = logger;
        private readonly IUserService _userService = userService;
        private readonly IEncrypter _encrypter = encrypter;

        [HttpGet]
        public async Task<
            ActionResult<DataListPaginationDTO<UserCreatedDTO?, ErrorDTO?>>
        > GetUsersAsync(
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
                encryptedFilter.Cursor = !string.IsNullOrEmpty(decrypted) ? decrypted : null;
                DataListPaginationDTO<UserCreatedDTO?, ErrorDTO?> responseDTO =
                    await _userService.GetUsersAsync(encryptedFilter);
                responseDTO.Next = !string.IsNullOrEmpty(responseDTO.Next)
                    ? _encrypter.EncryptString(responseDTO.Next)
                    : null;
                responseDTO.Previous = !string.IsNullOrEmpty(responseDTO.Previous)
                    ? _encrypter.EncryptString(responseDTO.Previous)
                    : null;
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
    }
}
