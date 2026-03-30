using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BeachRehberi.API.Models;
using BeachRehberi.API.DTOs.Reservation;

namespace BeachRehberi.API.Services;

public interface IReservationService
{
    Task<ServiceResult<Reservation>> CreateAsync(CreateReservationDto dto, int userId);
    Task<List<ReservationListItemDto>> GetByUserAsync(int userId);
    Task<ServiceResult<bool>> CancelAsync(int id, int userId);
}
