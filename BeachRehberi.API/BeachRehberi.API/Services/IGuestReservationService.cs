using System.Threading.Tasks;
using BeachRehberi.API.DTOs.Reservation;
using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface IGuestReservationService
{
    Task<ServiceResult<GuestReservationResponseDto>> CreateAsync(CreateGuestReservationDto dto);
    Task<ServiceResult<GuestReservationDetailDto>> GetByConfirmationCodeAsync(string confirmationCode, string email);
    Task<ServiceResult<GuestReservationResponseDto>> CancelAsync(string confirmationCode, string email);
    Task<ServiceResult<GuestReservationResponseDto>> ProcessPaymentAsync(string confirmationCode);
}
