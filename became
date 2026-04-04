[ApiController]
public class ReservationsController : ControllerBase
{
    // ...

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(
        int id,
        string firstName = null,
        string lastName = null,
        string email = null,
        string phone = null)
    {
        var reservation = await _reservationService.GetByUserAsync(UserId);
        if (reservation == null) return NotFound(new { success = false, message = "Rezervasyon bulunamadı." });

        // ...
    }
}
