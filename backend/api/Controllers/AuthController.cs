using api.Data;
using api.DTO.Interfaces;
using api.DTO.ResponseDTO;
using api.DTO.SetttingsDTO;
using api.DTO.UsersDTO;
using api.Exceptions;
using api.Helpers;
using api.Helpers.Instances;
using api.Models;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        // IConfiguration config,
        // AppDbContext dbContext,
        // JwtTokenGenerator jwtTokenGenerator,
        IAuthService authService
    ) : MyBaseController
    {
        // private readonly IConfiguration _config = config;
        // private readonly AppDbContext _dbContext = dbContext;
        // private readonly JwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
        private readonly IAuthService _authService = authService;

        [AllowAnonymous]
        [HttpPost]
        [Route("signup")]
        public ActionResult<string> SignUp([FromBody] UserCreateDTO login)
        {
            //this code is incomplete for the result
            return StatusCode(StatusCodes.Status500InternalServerError, new { res = "hola" });
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
            catch (Exception res)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { msg = res });
            }
        }
    }
}
