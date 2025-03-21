using api.Data;
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
        IHasher hasher,
        IAuthService authService
    ) : ControllerBase
    {
        // private readonly IConfiguration _config = config;
        // private readonly AppDbContext _dbContext = dbContext;
        // private readonly JwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
        private readonly IAuthService _authService = authService;
        private readonly IHasher _hasher = hasher;

        [AllowAnonymous]
        [HttpPost]
        [Route("signup")]
        public async Task<ActionResult<string>> SignUp([FromBody] UserCreateDTO login)
        {
            //this code is incomplete for the result
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new { res = ModelState }
                    );
                }
                var user = new User
                {
                    Username = login.Username,
                    Email = login.Email,
                    Password = login.Password,
                };
                var validIds = await _dbContext.Roles.ToListAsync();
                var validIdsList = login.Roles.All(x => login.Roles.Contains(x));

                if (!validIdsList)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new { res = "You need to select a role" }
                    );
                }
                var RolesObj = validIds.Where(r => login.Roles.Contains(r.Id)).ToList();
                user.Roles = RolesObj;
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                transaction.Commit();
                return Ok(user);
            }
            catch (Exception excp)
            {
                transaction.Rollback();
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { res = excp.Message }
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
                return Ok(response);
            }
            catch (Exception res)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { msg = res });
            }
        }
    }
}
