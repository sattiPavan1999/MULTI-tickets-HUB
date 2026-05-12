using AutoMapper;
using TrainService.Core.DTOs;
using TrainService.Core.Models;

namespace TrainService.Core.Mapping;

public class TrainMappingProfile : Profile
{
    public TrainMappingProfile()
    {
        CreateMap<TrainStop, TrainStopResponse>();

        CreateMap<Train, TrainResponse>()
            .ForMember(d => d.Stops, o => o.MapFrom(s => s.Stops.OrderBy(x => x.StopNumber).ToList()));

        CreateMap<SeatAvailability, SeatAvailabilityResponse>();

        CreateMap<TrainBooking, TrainBookingResponse>()
            .ForMember(d => d.TrainName,        o => o.MapFrom(s => s.Train != null ? s.Train.TrainName : null))
            .ForMember(d => d.TrainNumber,      o => o.MapFrom(s => s.Train != null ? s.Train.TrainNumber : null))
            .ForMember(d => d.Source,           o => o.MapFrom(s => s.Train != null ? s.Train.Source : null))
            .ForMember(d => d.Destination,      o => o.MapFrom(s => s.Train != null ? s.Train.Destination : null))
            .ForMember(d => d.DepartureTime,    o => o.MapFrom(s => s.Train != null ? (DateTime?)s.Train.DepartureTime : null))
            .ForMember(d => d.ArrivalTime,      o => o.MapFrom(s => s.Train != null ? (DateTime?)s.Train.ArrivalTime : null))
            .ForMember(d => d.BoardingStation,  o => o.MapFrom(s => s.BoardingStation))
            .ForMember(d => d.AlightingStation, o => o.MapFrom(s => s.AlightingStation));
    }
}
