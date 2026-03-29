using Microsoft.AspNetCore.Mvc;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult HandleResult<T>(T? result)
    {
        if (result is null)
            return NotFound(new { isSuccess = false, message = "Kayıt bulunamadı." });

        return Ok(new { isSuccess = true, data = result });
    }
}
