using backend.Models.Riot;

namespace backend.Services;

public class RiotApiService
{
    private readonly HttpClient _platformClient;
    private readonly HttpClient _regionalClient;

    public RiotApiService(IHttpClientFactory httpClientFactory)
    {
        _platformClient = httpClientFactory.CreateClient("RiotPlatform");
        _regionalClient = httpClientFactory.CreateClient("RiotRegional");
    }

}