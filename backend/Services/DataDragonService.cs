namespace backend.Services;

public class DataDragonService
{
    private readonly HttpClient _httpClient;

    public DataDragonService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

}