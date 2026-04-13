using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("internal/cleanup")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class InternalCleanupController : ControllerBase
{
    private readonly CloudSchedulerRequestValidator _requestValidator;
    private readonly IRevokedTokenCleanupJob _cleanupJob;

    public InternalCleanupController(
        CloudSchedulerRequestValidator requestValidator,
        IRevokedTokenCleanupJob cleanupJob)
    {
        _requestValidator = requestValidator;
        _cleanupJob = cleanupJob;
    }

    [AllowAnonymous]
    [HttpPost("revoked-tokens")]
    public async Task<IActionResult> CleanupRevokedTokens(CancellationToken cancellationToken)
    {
        var authorized = await _requestValidator.IsAuthorizedAsync(Request, cancellationToken);
        if (!authorized)
        {
            return Unauthorized(ApiResponse.Fail("Unauthorized."));
        }

        var deletedCount = await _cleanupJob.CleanupAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { deletedCount }, "Revoked token cleanup completed."));
    }
}
