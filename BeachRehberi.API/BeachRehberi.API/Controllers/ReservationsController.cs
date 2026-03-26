using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    // POST api/reservations
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
    {
        var reservation = new Reservation
        {
            BeachId = dto.BeachId,
            UserName = dto.UserName,
            UserPhone = dto.UserPhone,
            UserEmail = dto.UserEmail ?? "",
            ReservationDate = dto.ReservationDate,
            PersonCount = dto.PersonCount,
            SunbedCount = dto.SunbedCount,
            Notes = dto.Notes ?? "",
            TotalPrice = dto.TotalPrice
        };

        var result = await _reservationService.CreateAsync(reservation);
        return Ok(new
        {
            result.Id,
            result.ConfirmationCode,
            result.Status,
            Message = "Rezervasyon başarıyla oluşturuldu!"
        });
    }

    // GET api/reservations/phone/05551234567
    [HttpGet("phone/{phone}")]
    public async Task<IActionResult> GetByPhone(string phone)
    {
        var reservations = await _reservationService.GetByPhoneAsync(phone);
        return Ok(reservations);
    }

    // GET api/reservations/code/BR-12345
    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var reservation = await _reservationService.GetByCodeAsync(code);
        if (reservation == null) return NotFound();
        return Ok(reservation);
    }

    // DELETE api/reservations/BR-12345
    [HttpDelete("{code}")]
    public async Task<IActionResult> Cancel(string code)
    {
        var result = await _reservationService.CancelAsync(code);
        if (!result) return NotFound();
        return Ok(new { Message = "Rezervasyon iptal edildi." });
    }
}

public class CreateReservationDto
{
    public int BeachId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserPhone { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public DateTime ReservationDate { get; set; }
    public int PersonCount { get; set; }
    public int SunbedCount { get; set; }
    public string? Notes { get; set; }
    public decimal TotalPrice { get; set; }
}