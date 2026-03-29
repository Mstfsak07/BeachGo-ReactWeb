using BeachRehberi.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult ToActionResult<T>(Result<T> result)
    {
        return result.StatusCode switch
        {
            200 => Ok(new { success = true, data = result.Data, message = result.Message }),
            201 => StatusCode(201, new { success = true, data = result.Data, message = result.Message }),
            400 => BadRequest(new { success = false, message = result.Message, errors = result.Errors }),
            401 => Unauthorized(new { success = false, message = result.Message }),
            403 => StatusCode(403, new { success = false, message = result.Message }),
            404 => NotFound(new { success = false, message = result.Message }),
            _ => StatusCode(result.StatusCode, new { success = false, message = result.Message, errors = result.Errors })
        };
    }

    protected IActionResult ToActionResult(Result result)
    {
        return result.StatusCode switch
        {
            200 => Ok(new { success = true, message = result.Message }),
            400 => BadRequest(new { success = false, message = result.Message, errors = result.Errors }),
            401 => Unauthorized(new { success = false, message = result.Message }),
            404 => NotFound(new { success = false, message = result.Message }),
            _ => StatusCode(result.StatusCode, new { success = false, message = result.Message })
        };
    }
}
