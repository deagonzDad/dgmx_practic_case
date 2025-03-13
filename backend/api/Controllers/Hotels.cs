using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class Hotels : ControllerBase
    {
        [HttpGet]
        public IActionResult Test(){
            
            try{
                var testObject = new {
                message = "hola",
                value = 42,
                isActive = true
                }; 
                return Ok(testObject);
            }catch(Exception response){
                return StatusCode(StatusCodes.Status500InternalServerError, new {response});
            }
        }
    }
}
