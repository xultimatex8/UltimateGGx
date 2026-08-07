using System.Net;

namespace backend.Exceptions;

public class RiotApiException(HttpStatusCode statusCode, string url, Exception? innerException = null)
    : Exception($"Riot API request to '{url}' failed with status {(int)statusCode}.", innerException)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public string Url { get; } = url;
}