namespace MovieService.Core.Models;

public class Cinema
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string City { get; set; }
    public required string Address { get; set; }

    // Navigation properties
    public ICollection<Screen> Screens { get; set; } = new List<Screen>();
}
