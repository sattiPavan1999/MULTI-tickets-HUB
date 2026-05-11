namespace AdminBFF.Core.Exceptions;

public class ProxyException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
