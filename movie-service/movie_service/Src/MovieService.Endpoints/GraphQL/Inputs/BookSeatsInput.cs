namespace MovieService.Endpoints.GraphQL.Inputs;

public class BookSeatsInput
{
    public int UserId { get; set; }
    public int ShowId { get; set; }
    public required int[] SelectedSeatIds { get; set; }
}
