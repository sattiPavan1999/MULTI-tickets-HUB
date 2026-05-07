namespace TrainService.Core.DTOs;

public class CancelBookingResponse
{
    public int Id { get; set; }
    public long Pnr { get; set; }
    public string Status { get; set; } = string.Empty;
}
