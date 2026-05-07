using System.ComponentModel.DataAnnotations;

namespace TrainService.Core.DTOs;

public class SearchTrainInput
{
    [Required]
    public string SourceStation { get; set; } = string.Empty;

    [Required]
    public string DestinationStation { get; set; } = string.Empty;

    [Required]
    public DateOnly TravelDate { get; set; }
}
