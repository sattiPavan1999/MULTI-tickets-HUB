using System.ComponentModel.DataAnnotations;

namespace TrainService.Core.DTOs;

public class CreateBookingInput
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int TrainId { get; set; }

    [Required]
    public DateOnly TravelDate { get; set; }

    [Required]
    public string SeatClass { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int NumberOfPassengers { get; set; }

    [Required]
    public List<PassengerDetail> PassengerDetails { get; set; } = new();
}

public class PassengerDetail
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(1, 150)]
    public int Age { get; set; }

    [Required]
    public string Gender { get; set; } = string.Empty;
}
