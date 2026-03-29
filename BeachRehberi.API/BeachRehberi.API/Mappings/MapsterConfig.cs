using Mapster;
using BeachRehberi.API.Models;
using BeachRehberi.API.DTOs.Reservation;

namespace BeachRehberi.API.Mappings;

public static class MapsterConfig
{
    public static void Register()
    {
        TypeAdapterConfig<Reservation, ReservationListItemDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ReservationDate, src => src.ReservationDate)
            .Map(dest => dest.BeachName, src => src.Beach != null ? src.Beach.Name : string.Empty);
    }
}
