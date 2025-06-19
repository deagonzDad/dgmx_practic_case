using System.Threading.Tasks;
using api.DTO.ResponseDTO;
using api.DTO.SetttingsDTO;
using api.DTO.UsersDTO;
using api.Helpers;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService, ILogger<AuthController> logger)
        : MyBaseController
    {
        private readonly IAuthService _authService = authService;
        private readonly ILogger<AuthController> _logger = logger;

        [AllowAnonymous]
        [HttpPost]
        [Route("signup")]
        public async Task<ActionResult<ResponseDTO<UserCreatedDTO?, ErrorDTO?>>> SignUp(
            [FromBody] UserCreateDTO userDTO
        )
        {
            //this code is incomplete for the result
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new { res = ModelState }
                    );
                }
                ResponseDTO<UserCreatedDTO?, ErrorDTO?> responseDTO =
                    await _authService.SignupAsync(userDTO);
                return CreateResponse(responseDTO);
            }
            catch (Exception ex)
            {
                string messageError = "Something goes wrong unexpectedly";
                _logger.LogError(ex, "{messageError}", messageError);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { msg = messageError }
                );
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<ResponseDTO<JWTTokenResDTO?, ErrorDTO?>>> Login(
            [FromBody] UserSignInDTO login
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
                ResponseDTO<JWTTokenResDTO?, ErrorDTO?> response = await _authService.LoginAsync(
                    login
                );
                return CreateResponse(response);
            }
            catch (Exception ex)
            {
                string messageError = "Something goes wrong unexpectedly";
                _logger.LogError(ex, "{messageError}", messageError);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { msg = messageError }
                );
            }
        }
    }
}
