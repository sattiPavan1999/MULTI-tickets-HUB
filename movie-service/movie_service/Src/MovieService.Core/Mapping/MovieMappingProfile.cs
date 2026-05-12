using AutoMapper;
using MovieService.Core.DTOs;
using MovieService.Core.Models;

namespace MovieService.Core.Mapping;

public class MovieMappingProfile : Profile
{
    public MovieMappingProfile()
    {
        CreateMap<Movie, MovieResponse>();
        CreateMap<Showtime, ShowtimeResponse>();
        CreateMap<MovieBooking, BookingResponse>()
            .ForMember(d => d.MovieTitle,   o => o.MapFrom(s => s.Showtime != null && s.Showtime.Movie != null ? s.Showtime.Movie.Title : null))
            .ForMember(d => d.ShowDate,     o => o.MapFrom(s => s.Showtime != null ? s.Showtime.ShowDate.ToString("yyyy-MM-dd") : null))
            .ForMember(d => d.ShowTime,     o => o.MapFrom(s => s.Showtime != null ? s.Showtime.ShowTime.ToString("HH:mm") : null))
            .ForMember(d => d.ScreenNumber, o => o.MapFrom(s => s.Showtime != null ? s.Showtime.ScreenNumber : null));
    }
}
