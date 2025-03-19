using api.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController(ILogger<ReservationController> logger) : ControllerBase
    {
        private readonly ILogger<ReservationController> _logger = logger;

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
                    isActive = true,
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
