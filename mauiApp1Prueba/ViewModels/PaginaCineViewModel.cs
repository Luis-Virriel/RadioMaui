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

        // Campos privados
        private bool _isLoading;
        private bool _hasError;
        private string _errorMessage = string.Empty;
        private string _searchText = string.Empty;
        private Genre _selectedGenre;
        private string _statusMessage = string.Empty;

        // Propiedades públicas
        public ObservableCollection<Movie> Movies { get; set; } = new();
        public ObservableCollection<Genre> Genres { get; set; } = new();

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

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
                if (SetProperty(ref _searchText, value))
                {
                    _ = PerformSearchAsync();
                }
            }
        }

        public Genre SelectedGenre
        {
            get => _selectedGenre;
            set
            {
                if (SetProperty(ref _selectedGenre, value))
                {
                    _ = FilterByGenreAsync();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool HasMovies => Movies?.Count > 0;
        public bool IsNotLoading => !IsLoading;

        // Comandos
        public ICommand LoadMoviesCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        public PaginaCineViewModel(IMovieService movieService)
        {
            _movieService = movieService ?? throw new ArgumentNullException(nameof(movieService));

            // Inicializar comandos
            LoadMoviesCommand = new Command(async () => await LoadInitialDataAsync());
            RefreshCommand = new Command(async () => await RefreshDataAsync());
            ClearFiltersCommand = new Command(ClearFilters);

            // Cargar datos iniciales
            _ = Task.Run(LoadInitialDataAsync);
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                IsLoading = true;
                HasError = false;
                StatusMessage = "Cargando próximos estrenos...";

                // Cargar géneros y películas en paralelo
                var genresTask = _movieService.GetGenresAsync();
                var moviesTask = _movieService.GetUpcomingMoviesAsync();

                await Task.WhenAll(genresTask, moviesTask);

                var genres = await genresTask;
                var movies = await moviesTask;

                // Actualizar UI en el hilo principal
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    // Limpiar y cargar géneros
                    Genres.Clear();
                    Genres.Add(new Genre { Id = 0, Name = "Todos los géneros" });
                    foreach (var genre in genres)
                    {
                        Genres.Add(genre);
                    }

                    // Cargar películas
                    Movies.Clear();
                    foreach (var movie in movies)
                    {
                        Movies.Add(movie);
                    }

                    UpdateStatusMessage();
                    OnPropertyChanged(nameof(HasMovies));
                });
            }
            catch (Exception ex)
            {
                await HandleErrorAsync($"Error al cargar datos: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshDataAsync()
        {
            SearchText = string.Empty;
            SelectedGenre = null;
            await LoadInitialDataAsync();
        }

        private async Task PerformSearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadUpcomingMoviesAsync();
                return;
            }

            if (SearchText.Length < 2)
                return;

            try
            {
                IsLoading = true;
                HasError = false;
                StatusMessage = $"Buscando \"{SearchText}\"...";

                var searchResults = await _movieService.SearchMoviesAsync(SearchText);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Movies.Clear();
                    foreach (var movie in searchResults)
                    {
                        Movies.Add(movie);
                    }

                    // Limpiar selección de género al buscar
                    SelectedGenre = Genres.FirstOrDefault();

                    UpdateStatusMessage();
                    OnPropertyChanged(nameof(HasMovies));
                });
            }
            catch (Exception ex)
            {
                await HandleErrorAsync($"Error en la búsqueda: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task FilterByGenreAsync()
        {
            if (SelectedGenre == null || SelectedGenre.Id == 0)
            {
                await LoadUpcomingMoviesAsync();
                return;
            }

            try
            {
                IsLoading = true;
                HasError = false;
                StatusMessage = $"Filtrando por género: {SelectedGenre.Name}...";

                var genreMovies = await _movieService.GetMoviesByGenreAsync(SelectedGenre.Id);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Movies.Clear();
                    foreach (var movie in genreMovies)
                    {
                        Movies.Add(movie);
                    }

                    // Limpiar búsqueda al filtrar por género
                    SearchText = string.Empty;

                    UpdateStatusMessage();
                    OnPropertyChanged(nameof(HasMovies));
                });
            }
            catch (Exception ex)
            {
                await HandleErrorAsync($"Error al filtrar por género: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadUpcomingMoviesAsync()
        {
            try
            {
                IsLoading = true;
                HasError = false;
                StatusMessage = "Cargando próximos estrenos...";

                var movies = await _movieService.GetUpcomingMoviesAsync();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Movies.Clear();
                    foreach (var movie in movies)
                    {
                        Movies.Add(movie);
                    }

                    UpdateStatusMessage();
                    OnPropertyChanged(nameof(HasMovies));
                });
            }
            catch (Exception ex)
            {
                await HandleErrorAsync($"Error al cargar películas: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedGenre = Genres?.FirstOrDefault();
        }

        private void UpdateStatusMessage()
        {
            var count = Movies?.Count ?? 0;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                StatusMessage = $"Resultados para \"{SearchText}\": {count} películas";
            }
            else if (SelectedGenre != null && SelectedGenre.Id > 0)
            {
                StatusMessage = $"Género: {SelectedGenre.Name} - {count} películas";
            }
            else
            {
                StatusMessage = $"Próximos estrenos: {count} películas";
            }
        }

        private async Task HandleErrorAsync(string errorMessage)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                HasError = true;
                ErrorMessage = errorMessage;
                StatusMessage = "Error al cargar datos";
                OnPropertyChanged(nameof(HasMovies));
            });
        }

        // Implementación de INotifyPropertyChanged
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
    }
}