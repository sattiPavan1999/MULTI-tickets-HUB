using AutoMapper;
using MovieService.Core.DTOs;
using MovieService.Core.Models;

namespace MovieService.Core.Mapping;

public class MovieMappingProfile : Profile
{
    public MovieMappingProfile()
    {
        CreateMap<Movie, MovieResponse>();
    }
}
