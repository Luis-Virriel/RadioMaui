using System.Text.Json;
using mauiApp1Prueba.Models;

namespace mauiApp1Prueba.Services
{
    public interface IMovieService
    {
        Task<IEnumerable<Movie>> GetUpcomingMoviesAsync();
        Task<IEnumerable<Movie>> SearchMoviesAsync(string query);
        Task<IEnumerable<Genre>> GetGenresAsync();
        Task<IEnumerable<Movie>> GetMoviesByGenreAsync(int genreId);
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
            // No configuramos BaseAddress para usar URLs completas
        }

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

                // Debug: Log the JSON response
                System.Diagnostics.Debug.WriteLine($"API Response: {json.Substring(0, Math.Min(200, json.Length))}...");

                var movieResponse = JsonSerializer.Deserialize<MovieResponse>(json, GetJsonOptions());

                if (movieResponse?.Results == null)
                {
                    throw new Exception("La respuesta de la API no contiene resultados válidos");
                }

                System.Diagnostics.Debug.WriteLine($"Películas obtenidas: {movieResponse.Results.Length}");

                return movieResponse.Results;
            }
            catch (JsonException jsonEx)
            {
                throw new Exception($"Error al procesar la respuesta JSON: {jsonEx.Message}", jsonEx);
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"Error de conexión: {httpEx.Message}", httpEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener próximos estrenos: {ex.Message}", ex);
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