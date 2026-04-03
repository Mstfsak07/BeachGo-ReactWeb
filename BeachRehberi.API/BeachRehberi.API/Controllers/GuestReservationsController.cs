using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
            var verificationId = await _otpService.SendOtpAsync(dto.Phone);
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
        var result = await _guestReservationService.CreateAsync(dto);
        return result.ToActionResult();
    }

    [HttpGet("{confirmationCode}")]
    public async Task<IActionResult> GetByConfirmationCode(string confirmationCode)
    {
        var result = await _guestReservationService.GetByConfirmationCodeAsync(confirmationCode);
        return result.ToActionResult();
    }

    [HttpPost("cancel/{confirmationCode}")]
    public async Task<IActionResult> Cancel(string confirmationCode)
    {
        var result = await _guestReservationService.CancelAsync(confirmationCode);
        return result.ToActionResult();
    }

    [HttpPost("mock-pay/{confirmationCode}")]
    public async Task<IActionResult> MockPay(string confirmationCode)
    {
        var result = await _guestReservationService.MockPayAsync(confirmationCode);
        return result.ToActionResult();
    }
}
