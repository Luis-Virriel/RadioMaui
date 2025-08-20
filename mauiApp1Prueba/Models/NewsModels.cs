using System.Text.Json.Serialization;

namespace mauiApp1Prueba.Models
{
    public class NewsDataResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("totalResults")]
        public int TotalResults { get; set; }

        [JsonPropertyName("results")]
        public List<NewsArticle>? Results { get; set; }

        [JsonPropertyName("nextPage")]
        public string? NextPage { get; set; }
    }

    public class NewsArticle
    {
        [JsonPropertyName("article_id")]
        public string? ArticleId { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("link")]
        public string? Link { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        // Cambiado a string para evitar error de JSON
        [JsonPropertyName("pubDate")]
        public string? PubDate { get; set; }

        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("source_id")]
        public string? SourceId { get; set; }

        // Propiedad calculada para parsear la fecha
        [JsonIgnore]
        public DateTime? PubDateParsed
        {
            get
            {
                if (DateTime.TryParse(PubDate, out var dt))
                    return dt;
                return null;
            }
        }
    }
}
