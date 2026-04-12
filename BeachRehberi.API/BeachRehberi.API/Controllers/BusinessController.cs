using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.Models.Enums;
using BeachRehberi.API.Services;
using BeachRehberi.API.Extensions;
using BeachRehberi.API.DTOs;

[Authorize(Roles = UserRoles.Business + "," + UserRoles.Admin)]
[EnableRateLimiting("fixed")]
[ApiController]
[Route("api/[controller]")]
public class BusinessController : ControllerBase
{
    private readonly IBusinessService _businessService;
    private readonly IGoogleCloudStorageService _googleCloudStorageService;
    private readonly BeachDbContext _db;

    public BusinessController(
        IBusinessService businessService,
        IGoogleCloudStorageService googleCloudStorageService,
        BeachDbContext db)
    {
        _businessService = businessService;
        _googleCloudStorageService = googleCloudStorageService;
        _db = db;
    }

    [HttpGet("reservations")]
    public async Task<IActionResult> GetMyReservations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] string? filterType = null,
        [FromQuery] string? filterStatus = null,
        [FromQuery] string? sortType = null)
    {
        var beachId = GetUserBeachId();
        if (beachId == -1) return "Kendi işletme yetkiniz bulunamadı, işlem reddedildi.".ToForbiddenApiResponse();

        var reservations = await _businessService.GetAllReservationsAsync(
            beachId,
            page,
            pageSize,
            search,
            filterType,
            filterStatus,
            sortType);
        return reservations.ToPagedApiResponse();
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetMyStats()
    {
        var beachId = GetUserBeachId();
        if (beachId == -1) return "Kendi işletme yetkiniz bulunamadı, işlem reddedildi.".ToForbiddenApiResponse();

        var stats = await _businessService.GetStatsAsync(beachId);
        return stats.ToOkApiResponse();
    }

    [HttpGet("beach")]
    public async Task<IActionResult> GetMyBeach()
    {
        var beachId = GetUserBeachId();
        if (beachId == -1) return "Kendi işletme yetkiniz bulunamadı, işlem reddedildi.".ToForbiddenApiResponse();

        var beach = await _businessService.GetBeachByIdAsync(beachId);
        return beach.ToOkApiResponse();
    }

    [HttpPut("beach")]
    public async Task<IActionResult> UpdateMyBeach([FromBody] UpdateBeachDto beachUpdate)
    {
        var beachId = GetUserBeachId();
        if (beachId == -1) return "Kendi işletme yetkiniz bulunamadı, işlem reddedildi.".ToForbiddenApiResponse();

        // sadece belirli alanların güncellenmesine izin ver
        var result = await _businessService.UpdateBeachDetailsAsync(beachId, beachUpdate);
        return result.ToActionResult();
    }

    [HttpPost("beach/photos/upload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadBeachPhoto([FromForm] UploadBeachPhotoRequest request, CancellationToken cancellationToken)
    {
        var beachId = GetUserBeachId();
        if (beachId == -1) return "Kendi işletme yetkiniz bulunamadı, işlem reddedildi.".ToForbiddenApiResponse();

        var beach = await _businessService.GetBeachByIdAsync(beachId);
        if (beach == null)
            return "Plaj bulunamadı.".ToNotFoundApiResponse();

        if (request.File == null || request.File.Length == 0)
            return "Dosya gerekli.".ToBadRequestApiResponse();

        if (request.File.Length > 10 * 1024 * 1024)
            return "Dosya boyutu 10 MB sinirini asamaz.".ToBadRequestApiResponse();

        var allowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        var extension = Path.GetExtension(request.File.FileName);
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

        if (!allowedContentTypes.Contains(request.File.ContentType) || string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
            return "Yalnizca jpg, png veya webp resimleri yuklenebilir.".ToBadRequestApiResponse();

        var objectName = $"beach-photos/{beachId}/{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        try
        {
            await using var stream = request.File.OpenReadStream();
            var publicUrl = await _googleCloudStorageService.UploadPublicImageAsync(
                stream,
                objectName,
                request.File.ContentType,
                cancellationToken);

            var photo = new BeachPhoto(beachId, publicUrl, request.Caption ?? string.Empty, request.IsCover);
            _db.Photos.Add(photo);

            if (request.IsCover || string.IsNullOrWhiteSpace(beach.CoverImageUrl))
            {
                beach.CoverImageUrl = publicUrl;
            }

            await _db.SaveChangesAsync(cancellationToken);

            return new UploadBeachPhotoResponse
            {
                PhotoId = photo.Id,
                Url = photo.Url,
                Caption = photo.Caption,
                IsCover = request.IsCover
            }.ToOkApiResponse("Fotoğraf başarıyla yüklendi.");
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ApiResponse<object>.Fail(ex.Message, 503));
        }
    }

    [HttpPut("reservations/{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var beachId = GetUserBeachId();
        if (beachId == -1) return "Kendi işletme yetkiniz bulunamadı, işlem reddedildi.".ToForbiddenApiResponse();

        var result = await _businessService.UpdateReservationStatusAsync(id, beachId, ReservationStatus.Approved);
        return result.ToActionResult();
    }

    [HttpPut("reservations/{id}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] string? comment)
    {
        var beachId = GetUserBeachId();
        if (beachId == -1) return "Kendi işletme yetkiniz bulunamadı, işlem reddedildi.".ToForbiddenApiResponse();

        var result = await _businessService.UpdateReservationStatusAsync(id, beachId, ReservationStatus.Rejected, comment);
        return result.ToActionResult();
    }

    [HttpPut("reservations/{id}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] string? comment)
    {
        var beachId = GetUserBeachId();
        if (beachId == -1) return "Kendi işletme yetkiniz bulunamadı, işlem reddedildi.".ToForbiddenApiResponse();

        var result = await _businessService.UpdateReservationStatusAsync(id, beachId, ReservationStatus.Cancelled, comment);
        return result.ToActionResult();
    }

    private int GetUserBeachId()
    {
        var claim = User.FindFirst("BeachId")?.Value;
        return int.TryParse(claim, out int beachId) ? beachId : -1;
    }
}
