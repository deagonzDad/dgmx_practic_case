using api.DTO.Interfaces;
using api.DTO.ResponseDTO;
using Microsoft.AspNetCore.Mvc;

namespace api.Helpers;

public class MyBaseController : ControllerBase
{
    public ActionResult<ResponseDTO<TData, ErrorDTO?>> CreateResponse<TData>(
        ResponseDTO<TData, ErrorDTO?> response
    )
        where TData : IResponseData?
    {
        if (response.Success)
        {
            return Ok(response);
        }
        else if (response.Code == 400)
        {
            return BadRequest(response);
        }
        else if (response.Code == 401)
        {
            return Unauthorized(response);
        }
        else
        {
            return BadRequest(response);
        }
    }
}
