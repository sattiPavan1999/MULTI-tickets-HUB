namespace TrainService.Core.Services;

public interface IAuditService
{
    void LogSearch(string sourceStation, string destinationStation, DateOnly travelDate, int resultCount, int? userId = null);
    void LogBookingCreation(long pnr, int userId, int trainId, string seatClass, int passengerCount, decimal totalAmount);
    void LogBookingCancellation(long pnr, int userId, int bookingId);
    void LogError(string operation, string errorMessage, Exception? exception = null);
}
