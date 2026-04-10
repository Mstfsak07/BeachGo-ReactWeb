using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using BeachRehberi.API.DTOs.Reservation;
using BeachRehberi.API.Services;
using BeachRehberi.API.Extensions;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class GuestReservationsController : ControllerBase
{
    private readonly IOtpService _otpService;
    private readonly IGuestReservationService _guestReservationService;

    public GuestReservationsController(IOtpService otpService, IGuestReservationService guestReservationService)
    {
        _otpService = otpService;
        _guestReservationService = guestReservationService;
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto)
    {
        try
        {
            var verificationId = await _otpService.SendOtpAsync(dto.Email);
            return Ok(new { success = true, data = new SendOtpResponseDto { VerificationId = verificationId } });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        var verified = await _otpService.VerifyOtpAsync(dto.VerificationId, dto.Code);
        return Ok(new { success = true, data = new VerifyOtpResponseDto { Verified = verified } });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGuestReservationDto dto)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                dto.LoggedInUserId = userId;
            }
        }
        var result = await _guestReservationService.CreateAsync(dto);
        return result.ToActionResult();
    }

    [HttpGet("{confirmationCode}")]
    public async Task<IActionResult> GetByConfirmationCode(string confirmationCode, [FromQuery] string email)
    {
        var result = await _guestReservationService.GetByConfirmationCodeAsync(confirmationCode, email);
        return result.ToActionResult();
    }

    [HttpPost("cancel/{confirmationCode}")]
    [EnableRateLimiting("guest-cancel")]
    public async Task<IActionResult> Cancel(string confirmationCode, [FromBody] CancelGuestReservationDto dto)
    {
        var result = await _guestReservationService.CancelAsync(confirmationCode, dto.Email);
        return result.ToActionResult();
    }

    [HttpPost("pay/{confirmationCode}")]
    public async Task<IActionResult> Pay(string confirmationCode)
    {
        var result = await _guestReservationService.ProcessPaymentAsync(confirmationCode);
        return result.ToActionResult();
    }
}
