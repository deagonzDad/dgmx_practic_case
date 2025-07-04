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
            return Ok(response);
        else if (response.Code == 400)
            return BadRequest(response);
        else if (response.Code == 401)
            return Unauthorized(response);
        else if (response.Code == 404)
            return NotFound(response);
        else
            return BadRequest(response);
    }

    public ActionResult<DataListPaginationDTO<TData, ErrorDTO?>> CreateListResponse<TData>(
        DataListPaginationDTO<TData, ErrorDTO?> response
    )
        where TData : IResponseData?
    {
        if (response.Error == null)
        {
            return Ok(response);
        }
        else if (response.Error.ApiErrorCode == 401)
        {
            return Unauthorized(response);
        }
        else if (response.Error.ApiErrorCode == 400)
        {
            return BadRequest(response);
        }
        else
        {
            return BadRequest(response);
        }
    }
}
