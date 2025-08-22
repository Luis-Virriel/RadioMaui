using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using mauiApp1Prueba.Models;
using mauiApp1Prueba.Services;

namespace mauiApp1Prueba.ViewModels
{
    public class PaginaCineViewModel : INotifyPropertyChanged
    {
        private readonly IMovieService _movieService;
        private bool _isLoading;
        private bool _hasError;
        private string _errorMessage = string.Empty;
        private string _searchText = string.Empty;
        private string _statusMessage = string.Empty;
        private Genre _selectedGenre;
        private MovieViewType _selectedViewType = MovieViewType.All;

        // Colecciones separadas para mejor rendimiento
        private readonly List<Movie> _allMovies = new();
        private readonly List<Movie> _nowPlayingMovies = new();
        private readonly List<Movie> _upcomingMovies = new();

        public PaginaCineViewModel(IMovieService movieService)
        {
            _movieService = movieService;

            // Inicializar colecciones
            Movies = new ObservableCollection<Movie>();
            Genres = new ObservableCollection<Genre>();
            AllGenreOptions = new ObservableCollection<Genre>();

            // Inicializar comandos
            LoadMoviesCommand = new Command(async () => await LoadMoviesAsync());
            RefreshCommand = new Command(async () => await RefreshAsync());
            ClearFiltersCommand = new Command(async () => await ClearFiltersAsync());
            SearchCommand = new Command(async () => await SearchMoviesAsync());
            ShowTrailerCommand = new Command<Movie>(async (movie) => await ShowTrailerAsync(movie));
            ChangeViewTypeCommand = new Command<string>(async (viewType) => await ChangeViewTypeAsync(viewType));

            // Nuevos comandos para géneros
            SelectGenreCommand = new Command<Genre>(async (genre) => await SelectGenreAsync(genre));
            ClearGenreCommand = new Command(async () => await ClearGenreAsync());

            // Cargar géneros al inicializar
            _ = Task.Run(LoadGenresAsync);
        }

        #region Propiedades

        public ObservableCollection<Movie> Movies { get; }
        public ObservableCollection<Genre> Genres { get; }
        public ObservableCollection<Genre> AllGenreOptions { get; private set; }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                OnPropertyChanged(nameof(IsNotLoading));
            }
        }

        public bool IsNotLoading => !IsLoading;

        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);

                // Búsqueda automática con debounce
                _ = Task.Run(async () =>
                {
                    await Task.Delay(500); // Debounce de 500ms
                    if (_searchText == value) // Solo buscar si el texto no ha cambiado
                    {
                        await SearchMoviesAsync();
                    }
                });
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public Genre SelectedGenre
        {
            get => _selectedGenre;
            set
            {
                SetProperty(ref _selectedGenre, value);
                OnPropertyChanged(nameof(HasSelectedGenre));
                _ = Task.Run(async () => await FilterMoviesAsync());
            }
        }

        // Nueva propiedad para saber si hay un género seleccionado
        public bool HasSelectedGenre => SelectedGenre != null;

        public MovieViewType SelectedViewType
        {
            get => _selectedViewType;
            set
            {
                SetProperty(ref _selectedViewType, value);
                OnPropertyChanged(nameof(ViewTypeTitle));
                _ = Task.Run(async () => await FilterMoviesAsync());
            }
        }

        public string ViewTypeTitle
        {
            get
            {
                return SelectedViewType switch
                {
                    MovieViewType.NowPlaying => "🎬 En Cartelera",
                    MovieViewType.Upcoming => "🎯 Próximos Estrenos",
                    _ => "🎪 Todas las Películas"
                };
            }
        }

        public bool HasMovies => Movies.Count > 0;

        #endregion

        #region Comandos

        public ICommand LoadMoviesCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ShowTrailerCommand { get; }
        public ICommand ChangeViewTypeCommand { get; }

        // Nuevos comandos para géneros
        public ICommand SelectGenreCommand { get; }
        public ICommand ClearGenreCommand { get; }

        #endregion

        #region Métodos públicos

        public async Task InitializeAsync()
        {
            await LoadMoviesAsync();
        }

        #endregion

        #region Métodos privados

        private async Task LoadMoviesAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                HasError = false;
                StatusMessage = "Cargando películas...";

                // Cargar ambos tipos de películas en paralelo
                var nowPlayingTask = _movieService.GetNowPlayingMoviesAsync();
                var upcomingTask = _movieService.GetUpcomingMoviesAsync();

                await Task.WhenAll(nowPlayingTask, upcomingTask);

                // Limpiar y actualizar colecciones
                _nowPlayingMovies.Clear();
                _upcomingMovies.Clear();
                _allMovies.Clear();

                _nowPlayingMovies.AddRange(await nowPlayingTask);
                _upcomingMovies.AddRange(await upcomingTask);
                _allMovies.AddRange(_nowPlayingMovies);
                _allMovies.AddRange(_upcomingMovies);

                // Aplicar filtros actuales
                await FilterMoviesAsync();

                StatusMessage = $"Se encontraron {Movies.Count} películas";
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Error al cargar películas: {ex.Message}";
                StatusMessage = "Error al cargar películas";
                System.Diagnostics.Debug.WriteLine($"Error: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshAsync()
        {
            // Limpiar cachés y recargar
            _allMovies.Clear();
            _nowPlayingMovies.Clear();
            _upcomingMovies.Clear();

            await LoadMoviesAsync();
        }

        private async Task ClearFiltersAsync()
        {
            SearchText = string.Empty;
            SelectedGenre = null;
            SelectedViewType = MovieViewType.All;

            await FilterMoviesAsync();
        }

        // Nuevo método para seleccionar género
        private async Task SelectGenreAsync(Genre genre)
        {
            // Si es el género "Todos" (Id = 0), limpiar selección
            if (genre != null && genre.Id == 0)
            {
                SelectedGenre = null;
            }
            else
            {
                SelectedGenre = genre;
            }
            await FilterMoviesAsync();
        }

        // Nuevo método para limpiar solo el género
        private async Task ClearGenreAsync()
        {
            SelectedGenre = null;
            await FilterMoviesAsync();
        }

        private async Task SearchMoviesAsync()
        {
            if (IsLoading) return;

            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    await FilterMoviesAsync();
                    return;
                }

                IsLoading = true;
                StatusMessage = "Buscando películas...";

                var searchResults = await _movieService.SearchMoviesAsync(SearchText);

                Movies.Clear();
                foreach (var movie in searchResults.Take(20)) // Limitar resultados
                {
                    Movies.Add(movie);
                }

                StatusMessage = $"Se encontraron {Movies.Count} resultados para '{SearchText}'";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error en la búsqueda: {ex.Message}";
                StatusMessage = "Error en la búsqueda";
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(HasMovies));
            }
        }

        private async Task FilterMoviesAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Seleccionar la colección base según el tipo de vista
                    IEnumerable<Movie> baseMovies = SelectedViewType switch
                    {
                        MovieViewType.NowPlaying => _nowPlayingMovies,
                        MovieViewType.Upcoming => _upcomingMovies,
                        _ => _allMovies
                    };

                    // Aplicar filtro de género si está seleccionado
                    if (SelectedGenre != null)
                    {
                        baseMovies = baseMovies.Where(m => m.GenreIds.Contains(SelectedGenre.Id));
                    }

                    // Actualizar la UI en el hilo principal
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Movies.Clear();
                        foreach (var movie in baseMovies.OrderByDescending(m => m.Popularity))
                        {
                            Movies.Add(movie);
                        }

                        OnPropertyChanged(nameof(HasMovies));

                        // Actualizar mensaje de estado
                        var categoryText = SelectedViewType switch
                        {
                            MovieViewType.NowPlaying => "en cartelera",
                            MovieViewType.Upcoming => "próximas",
                            _ => "total"
                        };

                        var genreText = SelectedGenre != null ? $" de {SelectedGenre.Name}" : "";
                        StatusMessage = $"{Movies.Count} películas {categoryText}{genreText}";
                    });
                }
                catch (Exception ex)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ErrorMessage = $"Error al filtrar: {ex.Message}";
                    });
                }
            });
        }

        private async Task ShowTrailerAsync(Movie movie)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"ShowTrailerAsync llamado para: {movie?.Title}");

                if (movie == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error",
                        "No se pudo identificar la película seleccionada.", "OK");
                    return;
                }

                // Mostrar loading mientras verificamos si hay trailers
                IsLoading = true;
                StatusMessage = "Verificando disponibilidad de trailer...";

                try
                {
                    // Verificar si la película tiene trailers antes de navegar
                    var videos = await _movieService.GetMovieVideosAsync(movie.Id);
                    var hasTrailers = videos.Any(v => v.IsTrailer && v.IsYouTube);

                    if (!hasTrailers)
                    {
                        await Application.Current.MainPage.DisplayAlert("Sin trailer",
                            $"'{movie.Title}' no tiene trailers disponibles en YouTube.", "OK");
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine($"Navegando a trailer para: {movie.Title}");

                    // Navegar a la página de trailer
                    var parameters = new Dictionary<string, object>
                    {
                        ["Movie"] = movie
                    };

                    await Shell.Current.GoToAsync("trailer", parameters);

                    System.Diagnostics.Debug.WriteLine("Navegación completada");
                }
                catch (HttpRequestException httpEx)
                {
                    await Application.Current.MainPage.DisplayAlert("Error de conexión",
                        "No se pudo conectar al servicio de videos. Verifica tu conexión a internet.", "OK");
                }
                catch (Exception navEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error de navegación: {navEx.Message}");
                    await Application.Current.MainPage.DisplayAlert("Error",
                        $"No se pudo abrir el trailer: {navEx.Message}", "OK");
                }
                finally
                {
                    IsLoading = false;
                    StatusMessage = $"{Movies.Count} películas encontradas";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error general: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "Ocurrió un error inesperado al intentar ver el trailer.", "OK");
                IsLoading = false;
            }
        }

        private async Task ChangeViewTypeAsync(string viewTypeString)
        {
            if (Enum.TryParse<MovieViewType>(viewTypeString, out var viewType))
            {
                SelectedViewType = viewType;
                await FilterMoviesAsync();
            }
        }

        private async Task LoadGenresAsync()
        {
            try
            {
                var genres = await _movieService.GetGenresAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Genres.Clear();
                    AllGenreOptions.Clear();

                    // Agregar opción "Todos" primero
                    AllGenreOptions.Add(new Genre { Id = 0, Name = "Todos" });

                    foreach (var genre in genres)
                    {
                        Genres.Add(genre);
                        AllGenreOptions.Add(genre);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar géneros: {ex.Message}");
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}