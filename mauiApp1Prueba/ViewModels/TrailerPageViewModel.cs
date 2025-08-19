using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using mauiApp1Prueba.Models;
using mauiApp1Prueba.Services;

namespace mauiApp1Prueba.ViewModels
{
    public class TrailerPageViewModel : INotifyPropertyChanged
    {
        private readonly IMovieService _movieService;
        private bool _isLoading;
        private Movie _movie;
        private MovieVideo _selectedTrailer;

        public TrailerPageViewModel(IMovieService movieService)
        {
            _movieService = movieService;
            AvailableTrailers = new ObservableCollection<MovieVideo>();

            // Comandos
            CloseCommand = new Command(async () => await CloseAsync());
            OpenInYouTubeCommand = new Command(async () => await OpenInYouTubeAsync(), () => HasTrailer);
            SelectTrailerCommand = new Command<MovieVideo>(async (trailer) => await SelectTrailerAsync(trailer));
        }

        #region Propiedades

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public Movie Movie
        {
            get => _movie;
            set => SetProperty(ref _movie, value);
        }

        public MovieVideo SelectedTrailer
        {
            get => _selectedTrailer;
            set
            {
                SetProperty(ref _selectedTrailer, value);
                OnPropertyChanged(nameof(TrailerEmbedUrl));
                OnPropertyChanged(nameof(HasTrailer));
                ((Command)OpenInYouTubeCommand).ChangeCanExecute();
            }
        }

        public ObservableCollection<MovieVideo> AvailableTrailers { get; }

        public string TrailerEmbedUrl => SelectedTrailer?.YouTubeEmbedUrl ?? string.Empty;

        public bool HasTrailer => SelectedTrailer != null && !string.IsNullOrEmpty(SelectedTrailer.YouTubeEmbedUrl);

        public bool HasMultipleTrailers => AvailableTrailers.Count > 1;

        #endregion

        #region Comandos

        public ICommand CloseCommand { get; }
        public ICommand OpenInYouTubeCommand { get; }
        public ICommand SelectTrailerCommand { get; }

        #endregion

        #region Métodos públicos

        public async Task InitializeAsync(Movie movie)
        {
            try
            {
                if (movie == null)
                {
                    System.Diagnostics.Debug.WriteLine("Movie es null en InitializeAsync");
                    return;
                }

                Movie = movie;
                IsLoading = true;

                System.Diagnostics.Debug.WriteLine($"Iniciando carga de trailers para: {movie.Title}");

                // Limpiar trailers anteriores
                AvailableTrailers.Clear();
                SelectedTrailer = null;

                var videos = await _movieService.GetMovieVideosAsync(movie.Id);
                var trailers = videos.Where(v => v.IsTrailer && v.IsYouTube).ToList();

                System.Diagnostics.Debug.WriteLine($"Trailers encontrados: {trailers.Count}");

                // Agregar trailers a la colección en el hilo principal
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var trailer in trailers)
                    {
                        AvailableTrailers.Add(trailer);
                        System.Diagnostics.Debug.WriteLine($"Trailer: {trailer.Name} - Key: {trailer.Key}");
                    }

                    // Seleccionar el mejor trailer (oficial, mayor calidad)
                    SelectedTrailer = trailers
                        .OrderByDescending(t => t.Official)
                        .ThenByDescending(t => t.Size)
                        .FirstOrDefault();

                    if (SelectedTrailer != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Trailer seleccionado: {SelectedTrailer.Name}");
                        System.Diagnostics.Debug.WriteLine($"URL: {SelectedTrailer.YouTubeEmbedUrl}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No se encontraron trailers válidos");
                    }

                    OnPropertyChanged(nameof(HasMultipleTrailers));
                });
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"Error de conexión al cargar trailers: {httpEx.Message}");
                await ShowErrorMessage("Error de conexión",
                    "No se pudo conectar al servicio de trailers. Verifica tu conexión a internet.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar trailers: {ex.Message}");
                await ShowErrorMessage("Sin trailer",
                    $"No se pudo cargar el trailer de '{movie?.Title ?? "esta película"}'. Es posible que no esté disponible.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Métodos privados

        private async Task CloseAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Cerrando TrailerPage");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cerrar: {ex.Message}");

                // Fallback: intentar cerrar de otra manera
                try
                {
                    await Shell.Current.Navigation.PopAsync();
                }
                catch (Exception popEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en fallback close: {popEx.Message}");
                }
            }
        }

        private async Task OpenInYouTubeAsync()
        {
            try
            {
                if (SelectedTrailer != null && !string.IsNullOrEmpty(SelectedTrailer.YouTubeUrl))
                {
                    System.Diagnostics.Debug.WriteLine($"Abriendo YouTube: {SelectedTrailer.YouTubeUrl}");

                    var success = await Launcher.TryOpenAsync(SelectedTrailer.YouTubeUrl);

                    if (!success)
                    {
                        await ShowErrorMessage("No se pudo abrir",
                            "No se pudo abrir YouTube. Verifica que tengas la aplicación instalada o una conexión a internet.");
                    }
                }
                else
                {
                    await ShowErrorMessage("URL no válida",
                        "La URL del trailer no es válida.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al abrir YouTube: {ex.Message}");
                await ShowErrorMessage("Error",
                    "No se pudo abrir el trailer en YouTube.");
            }
        }

        private async Task SelectTrailerAsync(MovieVideo trailer)
        {
            try
            {
                if (trailer != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Seleccionando trailer: {trailer.Name}");
                    SelectedTrailer = trailer;

                    // Pequeña pausa para que la UI se actualice
                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al seleccionar trailer: {ex.Message}");
            }
        }

        private async Task ShowErrorMessage(string title, string message)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert(title, message, "OK");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error mostrando mensaje: {ex.Message}");
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