namespace TrainService.Core.DTOs;

public class BookingResponse
{
    public int Id { get; set; }
    public long Pnr { get; set; }
    public int UserId { get; set; }
    public int TrainId { get; set; }
    public DateOnly TravelDate { get; set; }
    public string SeatClass { get; set; } = string.Empty;
    public List<PassengerDetail> PassengerDetails { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime BookedAt { get; set; }
}
