using AutoMapper;
using TrainService.Core.DTOs;
using TrainService.Core.Models;

namespace TrainService.Core.Mapping;

public class TrainMappingProfile : Profile
{
    public TrainMappingProfile()
    {
        CreateMap<Train, TrainResponse>();
        CreateMap<SeatAvailability, SeatAvailabilityResponse>();
        CreateMap<TrainBooking, TrainBookingResponse>();
    }
}
