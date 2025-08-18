using System.Net.Http.Json;
using mauiApp1Prueba.Models;

public class NewsService
{
    private readonly HttpClient _httpClient;
    private const string ApiKey = "pub_0c88faa04d2a4d15b3896a6344492afe";

    public NewsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<NewsDataResponse?> GetUruguayNewsAsync(
        string? keyword = null,
        string? category = null,
        int size = 10,
        CancellationToken ct = default)
    {
        var q = System.Web.HttpUtility.ParseQueryString(string.Empty);
        q["apikey"] = ApiKey;
        q["country"] = "uy";
        q["language"] = "es";
        q["removeduplicate"] = "1";
        q["image"] = "1";
        q["size"] = Math.Clamp(size, 1, 10).ToString();

        if (!string.IsNullOrWhiteSpace(keyword))
            q["q"] = keyword;

        if (!string.IsNullOrWhiteSpace(category))
            q["category"] = category;

        var url = $"latest?{q}";
        var resp = await _httpClient.GetAsync(url, ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<NewsDataResponse>(cancellationToken: ct);
    }
}
