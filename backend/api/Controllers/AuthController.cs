using api.DTO.ResponseDTO;
using api.DTO.SettingsDTO;
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            ResponseDTO<UserCreatedDTO?, ErrorDTO?> responseDTO = await _authService.SignupAsync(
                userDTO
            );
            return CreateResponse(responseDTO);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<ResponseDTO<JWTTokenResDTO?, ErrorDTO?>>> Login(
            [FromBody] UserSignInDTO login
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            ResponseDTO<JWTTokenResDTO?, ErrorDTO?> response = await _authService.LoginAsync(login);
            return CreateResponse(response);
        }
    }
}
