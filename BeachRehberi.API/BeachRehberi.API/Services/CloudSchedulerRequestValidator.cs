using Google.Apis.Auth;
using Microsoft.AspNetCore.Http.Extensions;

namespace BeachRehberi.API.Services;

public sealed class CloudSchedulerRequestValidator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CloudSchedulerRequestValidator> _logger;

    public CloudSchedulerRequestValidator(IConfiguration configuration, ILogger<CloudSchedulerRequestValidator> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> IsAuthorizedAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var authHeader = request.Headers.Authorization.ToString();
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Cleanup endpoint request is missing a bearer token.");
            return false;
        }

        var schedulerInvokerEmail = _configuration["CloudScheduler:InvokerEmail"]
            ?? Environment.GetEnvironmentVariable("CLOUD_SCHEDULER_INVOKER_EMAIL");

        if (string.IsNullOrWhiteSpace(schedulerInvokerEmail))
        {
            _logger.LogError("Cloud Scheduler invoker email is not configured.");
            return false;
        }

        var configuredAudience = _configuration["CloudScheduler:CleanupAudience"]
            ?? Environment.GetEnvironmentVariable("CLOUD_SCHEDULER_CLEANUP_AUDIENCE");
        var expectedAudience = string.IsNullOrWhiteSpace(configuredAudience)
            ? request.GetDisplayUrl()
            : configuredAudience;

        try
        {
            var token = authHeader["Bearer ".Length..].Trim();
            var payload = await GoogleJsonWebSignature.ValidateAsync(token, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { expectedAudience }
            });

            var emailMatches = string.Equals(payload.Email, schedulerInvokerEmail, StringComparison.OrdinalIgnoreCase);
            if (!emailMatches || payload.EmailVerified != true)
            {
                _logger.LogWarning(
                    "Cleanup endpoint request rejected for email {Email}. Expected {ExpectedEmail}.",
                    payload.Email,
                    schedulerInvokerEmail);
                return false;
            }

            return true;
        }
        catch (Exception ex) when (ex is InvalidJwtException or ArgumentException)
        {
            _logger.LogWarning(ex, "Cleanup endpoint request contains an invalid OIDC token.");
            return false;
        }
    }
}
