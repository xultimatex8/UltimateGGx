namespace backend.Services;

public class RiotApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RiotApiService> _logger;

    public RiotApiService(HttpClient httpClient, ILogger<RiotApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

}