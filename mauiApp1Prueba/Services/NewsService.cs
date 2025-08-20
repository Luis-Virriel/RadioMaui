using System.Net.Http.Json;
using mauiApp1Prueba.Models;

public class NewsService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://newsdata.io/api/1/";
    private const string ApiKey = "pub_badbc32bb00c4a92b13e9ae558b9e53a";

    public NewsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<NewsDataResponse?> GetUruguayNewsAsync(
        string? keyword = null,
        int size = 10,
        string? nextPage = null)
    {
        var q = System.Web.HttpUtility.ParseQueryString(string.Empty);
        q["apikey"] = ApiKey;
        q["country"] = "uy";
        q["language"] = "es";
        q["removeduplicate"] = "1";
        q["image"] = "1";
        q["size"] = Math.Clamp(size, 1, 10).ToString();

        if (!string.IsNullOrWhiteSpace(keyword))
            q["q"] = keyword.Trim();

        if (!string.IsNullOrWhiteSpace(nextPage))
            q["page"] = nextPage;

        var url = $"latest?{q}";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<NewsDataResponse>(
            content,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        return result;
    }
}
