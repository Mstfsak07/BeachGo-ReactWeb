using Mapster;
using BeachRehberi.API.Models;
using BeachRehberi.API.DTOs;

namespace BeachRehberi.API.Mappings;

public static class MapsterConfig
{
    public static void Register()
    {
        TypeAdapterConfig<Reservation, ReservationListItemDto>.NewConfig()
            .Map(dest => dest.BeachName, src => src.Beach != null ? src.Beach.Name : string.Empty);
    }
}
