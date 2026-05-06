namespace MovieService.DTOs;

public class CinemaDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string City { get; set; }
    public required string Address { get; set; }
}
