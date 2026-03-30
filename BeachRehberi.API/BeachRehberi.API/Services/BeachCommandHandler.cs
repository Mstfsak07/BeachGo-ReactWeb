using MediatR;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.DTOs;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BeachRehberi.API.Services;

public record CreateBeachCommand(CreateBeachRequest BeachRequest) : IRequest<ServiceResult<BeachResponseDto>>;

public class BeachCommandHandler : IRequestHandler<CreateBeachCommand, ServiceResult<BeachResponseDto>>
{
    private readonly BeachDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BeachCommandHandler(BeachDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    private int? GetAuthenticatedUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
            return null;

        var userIdValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdValue, out int userId))
            return userId;

        return null;
    }

    private string? GetAuthenticatedUserRole()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirst(ClaimTypes.Role)?.Value;
    }

    public async Task<ServiceResult<BeachResponseDto>> Handle(CreateBeachCommand request, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (!userId.HasValue)
            return ServiceResult<BeachResponseDto>.FailureResult("Yetkisiz erişim. Lütfen giriş yapın.");

        var userRole = GetAuthenticatedUserRole();
        if (userRole != UserRoles.Admin && userRole != UserRoles.Business)
            return ServiceResult<BeachResponseDto>.FailureResult("Bu işlem için yetkiniz yok. Sadece Admin veya Business kullanıcıları plaj oluşturabilir.");

        // Validation
        if (string.IsNullOrWhiteSpace(request.BeachRequest.Name))
            return ServiceResult<BeachResponseDto>.FailureResult("Plaj adı zorunludur.");

        if (string.IsNullOrWhiteSpace(request.BeachRequest.Location))
            return ServiceResult<BeachResponseDto>.FailureResult("Konum zorunludur.");

        if (request.BeachRequest.Latitude < -90 || request.BeachRequest.Latitude > 90)
            return ServiceResult<BeachResponseDto>.FailureResult("Geçersiz enlem değeri.");

        if (request.BeachRequest.Longitude < -180 || request.BeachRequest.Longitude > 180)
            return ServiceResult<BeachResponseDto>.FailureResult("Geçersiz boylam değeri.");

        try
        {
            // OwnerId olarak giriş yapan işletme sahibinin/kullanıcının IDsi atanır. 0 ataması engellendi!
            var beach = new Beach(
                request.BeachRequest.Name,
                request.BeachRequest.Description,
                request.BeachRequest.Location,
                request.BeachRequest.Latitude,
                request.BeachRequest.Longitude,
                userId.Value 
            );

            _context.Beaches.Add(beach);
            await _context.SaveChangesAsync(cancellationToken);

            var beachDto = new DTOs.BeachResponseDto
            {
                Id = beach.Id,
                Name = beach.Name,
                Address = beach.Address,
                Description = beach.Description,
                Latitude = beach.Latitude,
                Longitude = beach.Longitude,
                Rating = beach.Rating,
                ReviewCount = beach.ReviewCount
            };

            return ServiceResult<BeachResponseDto>.SuccessResult(beachDto, "Plaj başarıyla oluşturuldu.");
        }
        catch (Exception ex)
        {
            return ServiceResult<BeachResponseDto>.FailureResult($"Plaj oluşturulurken hata oluştu: {ex.Message}");
        }
    }
}
