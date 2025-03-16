using api.Helpers;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class Hotels(ILogger<Hotels> logger) : ControllerBase
    {
        private readonly ILogger<Hotels> _logger = logger;
        [HttpGet]
        public IActionResult Test()
        {
            _logger.LoggerRequest("this is a message send by API Hotels");
            try
            {
                var testObject = new
                {
                    message = "hola",
                    value = 42,
                    isActive = true
                };
                return Ok(testObject);
            }
            catch (Exception response)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { response });
            }
        }
    }
}
