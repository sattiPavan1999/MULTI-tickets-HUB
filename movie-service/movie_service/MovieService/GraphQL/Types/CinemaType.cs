using MovieService.DTOs;

namespace MovieService.GraphQL.Types;

public class CinemaType
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string City { get; set; }
    public required string Address { get; set; }

    public static CinemaType FromDto(CinemaDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        City = dto.City,
        Address = dto.Address
    };
}
