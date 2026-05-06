using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainService.Models;

public class TrainBooking
{
    [Key]
    public int Id { get; set; }

    [Required]
    public long PNR { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int TrainId { get; set; }

    [ForeignKey(nameof(TrainId))]
    public Train Train { get; set; } = null!;

    [Required]
    public DateOnly TravelDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string SeatClass { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "jsonb")]
    public string PassengerDetails { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    [Required]
    public DateTime BookedAt { get; set; }
}
