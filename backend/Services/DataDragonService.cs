namespace backend.Services;

public class DataDragonService
{
    private readonly HttpClient _httpClient;

    public DataDragonService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetLatestVersionAsync(CancellationToken ct = default)
    {
        var versions = await _httpClient.GetFromJsonAsync<List<string>>(
            "api/versions.json", ct);
        return versions?.FirstOrDefault() ?? throw new Exception("Could not retrieve Data Dragon version");
    }
}