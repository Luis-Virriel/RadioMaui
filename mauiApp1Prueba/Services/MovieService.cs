using System.Text.Json;
using mauiApp1Prueba.Models;

namespace mauiApp1Prueba.Services
{
    public interface IMovieService
    {
        Task<IEnumerable<Movie>> GetUpcomingMoviesAsync();
        Task<IEnumerable<Movie>> GetNowPlayingMoviesAsync();
        Task<IEnumerable<Movie>> SearchMoviesAsync(string query);
        Task<IEnumerable<Genre>> GetGenresAsync();
        Task<IEnumerable<Movie>> GetMoviesByGenreAsync(int genreId);
        Task<IEnumerable<MovieVideo>> GetMovieVideosAsync(int movieId);
    }

    public class MovieService : IMovieService
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = "debc9f07cf1ea63de091b5c8927cfeec";
        private const string BaseUrl = "https://api.themoviedb.org/3";

        public MovieService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Obtiene películas que se estrenarán próximamente
        /// </summary>
        public async Task<IEnumerable<Movie>> GetUpcomingMoviesAsync()
        {
            try
            {
                var fullUrl = $"{BaseUrl}/movie/upcoming?api_key={ApiKey}&language=es-ES&page=1&region=US";

                System.Diagnostics.Debug.WriteLine($"Calling URL: {fullUrl}");

                var response = await _httpClient.GetAsync(fullUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Error Response: {errorContent}");
                    throw new HttpRequestException($"Error en la API: {response.StatusCode} - {errorContent}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var movieResponse = JsonSerializer.Deserialize<MovieResponse>(json, GetJsonOptions());

                if (movieResponse?.Results == null)
                {
                    throw new Exception("La respuesta de la API no contiene resultados válidos");
                }

                // Filtrar solo películas que realmente son próximas (fecha futura)
                var upcomingMovies = movieResponse.Results
                    .Where(m => DateTime.TryParse(m.ReleaseDate, out var releaseDate) &&
                               releaseDate > DateTime.Now)
                    .OrderBy(m => DateTime.Parse(m.ReleaseDate))
                    .ToArray();

                System.Diagnostics.Debug.WriteLine($"Próximas películas obtenidas: {upcomingMovies.Length}");

                return upcomingMovies;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener próximos estrenos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene películas actualmente en cartelera (recién estrenadas)
        /// </summary>
        public async Task<IEnumerable<Movie>> GetNowPlayingMoviesAsync()
        {
            try
            {
                var fullUrl = $"{BaseUrl}/movie/now_playing?api_key={ApiKey}&language=es-ES&page=1&region=US";

                System.Diagnostics.Debug.WriteLine($"Calling URL: {fullUrl}");

                var response = await _httpClient.GetAsync(fullUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Error en la API: {response.StatusCode} - {errorContent}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var movieResponse = JsonSerializer.Deserialize<MovieResponse>(json, GetJsonOptions());

                if (movieResponse?.Results == null)
                {
                    throw new Exception("La respuesta de la API no contiene resultados válidos");
                }

                // Filtrar películas estrenadas en los últimos 45 días
                var nowPlayingMovies = movieResponse.Results
                    .Where(m => DateTime.TryParse(m.ReleaseDate, out var releaseDate) &&
                               releaseDate <= DateTime.Now &&
                               releaseDate >= DateTime.Now.AddDays(-45))
                    .OrderByDescending(m => DateTime.Parse(m.ReleaseDate))
                    .ToArray();

                System.Diagnostics.Debug.WriteLine($"Películas en cartelera obtenidas: {nowPlayingMovies.Length}");

                return nowPlayingMovies;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener películas en cartelera: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene los videos/trailers de una película específica
        /// </summary>
        public async Task<IEnumerable<MovieVideo>> GetMovieVideosAsync(int movieId)
        {
            try
            {
                var fullUrl = $"{BaseUrl}/movie/{movieId}/videos?api_key={ApiKey}&language=es-ES";

                System.Diagnostics.Debug.WriteLine($"Getting videos for movie {movieId}: {fullUrl}");

                var response = await _httpClient.GetAsync(fullUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Error en la API: {response.StatusCode} - {errorContent}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var videoResponse = JsonSerializer.Deserialize<VideoResponse>(json, GetJsonOptions());

                if (videoResponse?.Results == null)
                {
                    return Enumerable.Empty<MovieVideo>();
                }

                // Filtrar solo trailers de YouTube y ordenar por calidad
                var trailers = videoResponse.Results
                    .Where(v => v.Type.Equals("Trailer", StringComparison.OrdinalIgnoreCase) &&
                               v.Site.Equals("YouTube", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(v => v.Size) // Priorizar mayor calidad
                    .ThenByDescending(v => v.Official) // Priorizar oficiales
                    .ToArray();

                System.Diagnostics.Debug.WriteLine($"Trailers encontrados: {trailers.Length}");

                return trailers;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener videos: {ex.Message}");
                return Enumerable.Empty<MovieVideo>();
            }
        }

        public async Task<IEnumerable<Movie>> SearchMoviesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<Movie>();

            try
            {
                var encodedQuery = Uri.EscapeDataString(query);
                var fullUrl = $"{BaseUrl}/search/movie?api_key={ApiKey}&language=es-ES&query={encodedQuery}&page=1";

                var response = await _httpClient.GetAsync(fullUrl);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error en la API: {response.StatusCode}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var movieResponse = JsonSerializer.Deserialize<MovieResponse>(json, GetJsonOptions());

                return movieResponse?.Results ?? Enumerable.Empty<Movie>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar películas: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Genre>> GetGenresAsync()
        {
            try
            {
                var fullUrl = $"{BaseUrl}/genre/movie/list?api_key={ApiKey}&language=es-ES";

                var response = await _httpClient.GetAsync(fullUrl);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error en la API: {response.StatusCode}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var genreResponse = JsonSerializer.Deserialize<GenreResponse>(json, GetJsonOptions());

                return genreResponse?.Genres ?? Enumerable.Empty<Genre>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener géneros: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Movie>> GetMoviesByGenreAsync(int genreId)
        {
            try
            {
                var fullUrl = $"{BaseUrl}/discover/movie?api_key={ApiKey}&language=es-ES&with_genres={genreId}&sort_by=popularity.desc&page=1";

                var response = await _httpClient.GetAsync(fullUrl);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error en la API: {response.StatusCode}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var movieResponse = JsonSerializer.Deserialize<MovieResponse>(json, GetJsonOptions());

                return movieResponse?.Results ?? Enumerable.Empty<Movie>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener películas por género: {ex.Message}", ex);
            }
        }

        private static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }
    }
}