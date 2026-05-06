using System.Diagnostics;
using AdminBFF.Models;
using HotChocolate;

namespace AdminBFF.GraphQL;

public class GraphQLErrorFilter : IErrorFilter
{
    private readonly ILogger<GraphQLErrorFilter> _logger;

    public GraphQLErrorFilter(ILogger<GraphQLErrorFilter> logger)
    {
        _logger = logger;
    }

    public IError OnError(IError error)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString();

        if (error.Exception is AdminBFFException adminException)
        {
            _logger.LogWarning(adminException, "Business exception occurred. TraceId: {TraceId}", traceId);

            return ErrorBuilder.New()
                .SetMessage(adminException.Message)
                .SetCode(adminException.ErrorCode)
                .SetExtension("statusCode", adminException.StatusCode)
                .SetExtension("traceId", traceId)
                .SetExtension("timestamp", DateTime.UtcNow)
                .RemoveException()
                .Build();
        }

        _logger.LogError(error.Exception, "Unexpected error occurred. TraceId: {TraceId}", traceId);

        return ErrorBuilder.New()
            .SetMessage("An internal server error occurred")
            .SetCode("INTERNAL_ERROR")
            .SetExtension("statusCode", 500)
            .SetExtension("traceId", traceId)
            .SetExtension("timestamp", DateTime.UtcNow)
            .RemoveException()
            .Build();
    }
}
