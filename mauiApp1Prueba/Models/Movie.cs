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

        // Propiedades calculadas existentes
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

        // Nuevas propiedades para categorización
        public bool IsUpcoming
        {
            get
            {
                if (DateTime.TryParse(ReleaseDate, out DateTime releaseDate))
                {
                    return releaseDate > DateTime.Now;
                }
                return false;
            }
        }

        public bool IsNowPlaying
        {
            get
            {
                if (DateTime.TryParse(ReleaseDate, out DateTime releaseDate))
                {
                    return releaseDate <= DateTime.Now &&
                           releaseDate >= DateTime.Now.AddDays(-45);
                }
                return false;
            }
        }

        public string MovieStatus
        {
            get
            {
                if (IsNowPlaying) return "🎬 EN CARTELERA";
                if (IsUpcoming) return "🎯 PRÓXIMO ESTRENO";
                return "🎪 DISPONIBLE";
            }
        }

        public Color StatusColor
        {
            get
            {
                if (IsNowPlaying) return Color.FromArgb("#e94560"); // Rojo
                if (IsUpcoming) return Color.FromArgb("#533483");   // Morado
                return Color.FromArgb("#0f3460");                   // Azul
            }
        }

        public string DaysUntilRelease
        {
            get
            {
                if (DateTime.TryParse(ReleaseDate, out DateTime releaseDate))
                {
                    var daysUntil = (releaseDate - DateTime.Now).Days;

                    if (daysUntil > 0)
                        return $"En {daysUntil} día{(daysUntil == 1 ? "" : "s")}";
                    else if (daysUntil == 0)
                        return "¡Estreno hoy!";
                    else if (daysUntil >= -7)
                        return "¡Recién estrenada!";
                    else
                        return $"Hace {Math.Abs(daysUntil)} días";
                }
                return string.Empty;
            }
        }

        // Propiedades para rating visual mejorado
        public string RatingStars
        {
            get
            {
                if (VoteAverage <= 0) return "☆☆☆☆☆";

                var stars = (int)Math.Round(VoteAverage / 2);
                var fullStars = new string('★', stars);
                var emptyStars = new string('☆', 5 - stars);
                return fullStars + emptyStars;
            }
        }

        public bool HasGoodRating => VoteAverage >= 7.0;
    }

    public class MovieVideo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("site")]
        public string Site { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("official")]
        public bool Official { get; set; }

        [JsonPropertyName("published_at")]
        public string PublishedAt { get; set; } = string.Empty;

        // Propiedades calculadas para YouTube
        public string YouTubeUrl => Site == "YouTube"
            ? $"https://www.youtube.com/watch?v={Key}"
            : string.Empty;

        public string YouTubeEmbedUrl => Site == "YouTube"
            ? $"https://www.youtube.com/embed/{Key}?autoplay=1&rel=0"
            : string.Empty;

        public string YouTubeThumbnailUrl => Site == "YouTube"
            ? $"https://img.youtube.com/vi/{Key}/maxresdefault.jpg"
            : string.Empty;

        public string QualityText
        {
            get
            {
                return Size switch
                {
                    1080 => "HD 1080p",
                    720 => "HD 720p",
                    480 => "SD 480p",
                    360 => "SD 360p",
                    _ => "Calidad estándar"
                };
            }
        }

        public bool IsTrailer => Type.Equals("Trailer", StringComparison.OrdinalIgnoreCase);
        public bool IsYouTube => Site.Equals("YouTube", StringComparison.OrdinalIgnoreCase);
        public bool IsHighQuality => Size >= 720;
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

    public class VideoResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("results")]
        public MovieVideo[] Results { get; set; } = Array.Empty<MovieVideo>();
    }

    public class GenreResponse
    {
        [JsonPropertyName("genres")]
        public Genre[] Genres { get; set; } = Array.Empty<Genre>();
    }

    // Enum para tipos de vista
    public enum MovieViewType
    {
        All,
        NowPlaying,
        Upcoming
    }

}