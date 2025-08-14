using System.Text.Json.Serialization;

namespace mauiApp1Prueba.Models
{
    public class Movie
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("overview")]
        public string Overview { get; set; } = string.Empty;

        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; } = string.Empty;

        [JsonPropertyName("poster_path")]
        public string PosterPath { get; set; } = string.Empty;

        [JsonPropertyName("backdrop_path")]
        public string BackdropPath { get; set; } = string.Empty;

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        [JsonPropertyName("vote_count")]
        public int VoteCount { get; set; }

        [JsonPropertyName("genre_ids")]
        public int[] GenreIds { get; set; } = Array.Empty<int>();

        [JsonPropertyName("adult")]
        public bool Adult { get; set; }

        [JsonPropertyName("original_language")]
        public string OriginalLanguage { get; set; } = string.Empty;

        [JsonPropertyName("original_title")]
        public string OriginalTitle { get; set; } = string.Empty;

        [JsonPropertyName("popularity")]
        public double Popularity { get; set; }

        [JsonPropertyName("video")]
        public bool Video { get; set; }

        // Propiedades calculadas
        public string FullPosterUrl => string.IsNullOrEmpty(PosterPath)
            ? string.Empty
            : $"https://image.tmdb.org/t/p/w500{PosterPath}";

        public string FullBackdropUrl => string.IsNullOrEmpty(BackdropPath)
            ? string.Empty
            : $"https://image.tmdb.org/t/p/w780{BackdropPath}";

        public string FormattedReleaseDate
        {
            get
            {
                if (DateTime.TryParse(ReleaseDate, out DateTime date))
                {
                    return date.ToString("dd 'de' MMMM 'de' yyyy",
                        new System.Globalization.CultureInfo("es-ES"));
                }
                return "Fecha no disponible";
            }
        }

        public string FormattedVoteAverage => VoteAverage > 0
            ? VoteAverage.ToString("F1")
            : "N/A";

        public bool HasPoster => !string.IsNullOrEmpty(PosterPath);

        public bool HasOverview => !string.IsNullOrEmpty(Overview);

        public string ShortOverview => Overview.Length > 150
            ? Overview.Substring(0, 147) + "..."
            : Overview;
    }

    public class Genre
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    // Respuestas de la API
    public class MovieResponse
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("results")]
        public Movie[] Results { get; set; } = Array.Empty<Movie>();

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("total_results")]
        public int TotalResults { get; set; }
    }

    public class GenreResponse
    {
        [JsonPropertyName("genres")]
        public Genre[] Genres { get; set; } = Array.Empty<Genre>();
    }
}