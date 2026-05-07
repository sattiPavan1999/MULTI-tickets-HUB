using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TrainService.Core.Services;

public class AuditService : IAuditService
{
    private readonly ILogger<AuditService> _logger;

    public AuditService(ILogger<AuditService> logger)
    {
        _logger = logger;
    }

    public void LogSearch(string sourceStation, string destinationStation, DateOnly travelDate, int resultCount, int? userId = null)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? "N/A";
        _logger.LogInformation(
            "[AUDIT] TrainSearch - TraceId: {TraceId}, UserId: {UserId}, Source: {Source}, Destination: {Destination}, Date: {Date}, Results: {ResultCount}",
            traceId, userId?.ToString() ?? "Anonymous", sourceStation, destinationStation, travelDate, resultCount);
    }

    public void LogBookingCreation(long pnr, int userId, int trainId, string seatClass, int passengerCount, decimal totalAmount)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? "N/A";
        _logger.LogInformation(
            "[AUDIT] BookingCreated - TraceId: {TraceId}, PNR: {PNR}, UserId: {UserId}, TrainId: {TrainId}, SeatClass: {SeatClass}, Passengers: {PassengerCount}, Amount: {Amount}",
            traceId, pnr, userId, trainId, seatClass, passengerCount, totalAmount);
    }

    public void LogBookingCancellation(long pnr, int userId, int bookingId)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? "N/A";
        _logger.LogInformation(
            "[AUDIT] BookingCancelled - TraceId: {TraceId}, PNR: {PNR}, UserId: {UserId}, BookingId: {BookingId}",
            traceId, pnr, userId, bookingId);
    }

    public void LogError(string operation, string errorMessage, Exception? exception = null)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? "N/A";
        if (exception != null)
        {
            _logger.LogError(exception,
                "[AUDIT] Error - TraceId: {TraceId}, Operation: {Operation}, Message: {Message}",
                traceId, operation, errorMessage);
        }
        else
        {
            _logger.LogError(
                "[AUDIT] Error - TraceId: {TraceId}, Operation: {Operation}, Message: {Message}",
                traceId, operation, errorMessage);
        }
    }
}
