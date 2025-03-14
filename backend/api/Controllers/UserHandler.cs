using api.Data;
using api.DTO.Users;
using api.Helpers;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserHandler(IConfiguration config, AppDbContext dbContext, JwtTokenGenerator jwtTokenGenerator) : ControllerBase
    {
        private readonly IConfiguration _config = config;
        private readonly AppDbContext _dbContext = dbContext;
        private readonly JwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
        [AllowAnonymous]
        [HttpPost]
        [Route("login")]

        public async Task<ActionResult<string>> Login([FromBody] UserCreateDTO login){
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { res = ModelState });
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
                    return StatusCode(StatusCodes.Status500InternalServerError, new { res = "You need to select a role" });
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
                return StatusCode(StatusCodes.Status500InternalServerError, new {res = excp.Message});
            }
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("signup")]
        public async Task<ActionResult<string>> SignIn([FromBody] UserSignInDTO signIn)
        {
            // using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                if(!ModelState.IsValid){
                    return StatusCode(StatusCodes.Status500InternalServerError, new { res = ModelState });
                }
                User? user = await _dbContext.Users.Where(r=>r.Username == signIn.Username)
                    .Include(a=>a.Roles).FirstOrDefaultAsync();
                if(user == null || !PasswordHasher.VerifyPassword(signIn.Password, user.Password))
                {
                    return Unauthorized("Invalid Credentials");
                }
                string token = _jwtTokenGenerator.GenerateToken(user);
                return Ok(new {Token = token});

            }catch(Exception res)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new {msg = res});
            }
        }
    }
}
