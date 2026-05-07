using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainService.Core.Models;

public class Train
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string TrainNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string TrainName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string SourceStation { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string DestinationStation { get; set; } = string.Empty;

    [Required]
    public TimeSpan DepartureTime { get; set; }

    [Required]
    public TimeSpan ArrivalTime { get; set; }

    [Required]
    [Column(TypeName = "jsonb")]
    public string TotalSeats { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "jsonb")]
    public string Fares { get; set; } = string.Empty;

    public ICollection<TrainBooking> Bookings { get; set; } = new List<TrainBooking>();
}
