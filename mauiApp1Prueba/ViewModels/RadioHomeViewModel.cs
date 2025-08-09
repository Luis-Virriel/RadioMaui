using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using mauiApp1Prueba.Services;

namespace mauiApp1Prueba.ViewModels
{
    public class RadioHomeViewModel : INotifyPropertyChanged
    {
        private readonly IAudioService _audioService;

        // Estado del reproductor
        private bool _isPlaying = false;
        private bool _isLoading = false;
        private bool _isMuted = false;
        private bool _isFavorite = false;
        private bool _isLive = true;
        private bool _showVolumeControl = false;
        private bool _showConnectionStatus = true;
        private double _volume = 50;

        // Información de la radio
        private string _currentShow = "Radio en Vivo";
        private string _currentTime = DateTime.Now.ToString("HH:mm");
        private string _connectionStatus = "Conectado";
        private Color _connectionStatusColor = Colors.Green;
        private string _welcomeMessage = "¡Bienvenido a Radio Punta!";

        // URL del stream de radio - Metropolis
        private const string RadioStreamUrl = "https://metropolis-web-1.nty.uy";

        public RadioHomeViewModel(IAudioService audioService)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));

            // Suscribirse a eventos del servicio de audio
            _audioService.PlayingStateChanged += OnAudioPlayingStateChanged;
            _audioService.ErrorOccurred += OnAudioErrorOccurred;

            InitializeCommands();
            _ = InitializeAsync();
        }

        #region Properties

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                SetProperty(ref _isPlaying, value);
                OnPropertyChanged(nameof(PlayPauseIcon));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                SetProperty(ref _isMuted, value);
                OnPropertyChanged(nameof(VolumeIcon));
            }
        }

        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                SetProperty(ref _isFavorite, value);
                OnPropertyChanged(nameof(FavoriteIcon));
            }
        }

        public bool IsLive
        {
            get => _isLive;
            set => SetProperty(ref _isLive, value);
        }

        public bool ShowVolumeControl
        {
            get => _showVolumeControl;
            set => SetProperty(ref _showVolumeControl, value);
        }

        public bool ShowConnectionStatus
        {
            get => _showConnectionStatus;
            set => SetProperty(ref _showConnectionStatus, value);
        }

        public double Volume
        {
            get => _volume;
            set
            {
                SetProperty(ref _volume, value);
                Preferences.Set("radio_volume", value);

                // Aplicar volumen al servicio de audio
                _ = _audioService.SetVolumeAsync(value / 100.0); // Convertir 0-100 a 0-1
            }
        }

        public string CurrentShow
        {
            get => _currentShow;
            set => SetProperty(ref _currentShow, value);
        }

        public string CurrentTime
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        public Color ConnectionStatusColor
        {
            get => _connectionStatusColor;
            set => SetProperty(ref _connectionStatusColor, value);
        }

        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        // Iconos dinámicos
        public string PlayPauseIcon => IsPlaying ? "⏸️" : "▶️";
        public string VolumeIcon => IsMuted ? "🔇" : "🔊";
        public string FavoriteIcon => IsFavorite ? "❤️" : "🤍";

        #endregion

        #region Commands

        public ICommand PlayPauseCommand { get; private set; } = null!;
        public ICommand ToggleMuteCommand { get; private set; } = null!;
        public ICommand ToggleFavoriteCommand { get; private set; } = null!;
        public ICommand ViewScheduleCommand { get; private set; } = null!;
        public ICommand ViewPodcastsCommand { get; private set; } = null!;
        public ICommand ViewNewsCommand { get; private set; } = null!;
        public ICommand ContactCommand { get; private set; } = null!;

        #endregion

        #region Methods

        private void InitializeCommands()
        {
            PlayPauseCommand = new Command(async () => await ExecutePlayPauseCommand());
            ToggleMuteCommand = new Command(() => ExecuteToggleMuteCommand());
            ToggleFavoriteCommand = new Command(() => ExecuteToggleFavoriteCommand());
            ViewScheduleCommand = new Command(async () => await ExecuteViewScheduleCommand());
            ViewPodcastsCommand = new Command(async () => await ExecuteViewPodcastsCommand());
            ViewNewsCommand = new Command(async () => await ExecuteViewNewsCommand());
            ContactCommand = new Command(async () => await ExecuteContactCommand());
        }

        private async Task InitializeAsync()
        {
            try
            {
                // Cargar preferencias guardadas
                Volume = Preferences.Get("radio_volume", 50.0);
                IsFavorite = Preferences.Get("radio_is_favorite", false);

                // Verificar conectividad
                await CheckConnectivityAsync();

                // Inicializar reloj
                _ = StartClockAsync();

                // Cargar información del programa actual
                await LoadCurrentShowInfoAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing: {ex.Message}");
            }
        }

        private void OnAudioPlayingStateChanged(object? sender, bool isPlaying)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsPlaying = isPlaying;
                IsLoading = false;

                if (isPlaying)
                {
                    ConnectionStatus = "En vivo";
                    ConnectionStatusColor = Colors.Green;
                }
                else
                {
                    ConnectionStatus = "Desconectado";
                    ConnectionStatusColor = Colors.Gray;
                }
            });
        }

        private void OnAudioErrorOccurred(object? sender, string error)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsPlaying = false;
                IsLoading = false;
                ConnectionStatus = "Error";
                ConnectionStatusColor = Colors.Red;
            });
        }

        public async Task StopRadioAsync()
        {
            await _audioService.StopAsync();
        }

        private async Task ExecutePlayPauseCommand()
        {
            try
            {
                if (IsPlaying)
                {
                    // Pausar
                    await _audioService.StopAsync();
                }
                else
                {
                    // Reproducir
                    IsLoading = true;
                    ConnectionStatus = "Conectando...";
                    ConnectionStatusColor = Colors.Orange;

                    var success = await _audioService.PlayStreamAsync(RadioStreamUrl);
                    if (!success)
                    {
                        IsLoading = false;
                        ConnectionStatus = "Error";
                        ConnectionStatusColor = Colors.Red;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                IsLoading = false;
                ConnectionStatus = "Error";
                ConnectionStatusColor = Colors.Red;
            }
        }

        private void ExecuteToggleMuteCommand()
        {
            IsMuted = !IsMuted;

            if (IsMuted)
            {
                // Silenciar
                Preferences.Set("radio_volume_before_mute", Volume);
                Volume = 0;
            }
            else
            {
                // Restaurar volumen
                Volume = Preferences.Get("radio_volume_before_mute", 50.0);
            }
        }

        private void ExecuteToggleFavoriteCommand()
        {
            IsFavorite = !IsFavorite;
            Preferences.Set("radio_is_favorite", IsFavorite);

            var message = IsFavorite ? "¡Radio agregada a favoritos!" : "Radio removida de favoritos";
            ShowToast(message);
        }

        private async Task ExecuteViewScheduleCommand()
        {
            await ShowAlertAsync("Programación", "Próximamente: Horarios y programas de Radio Punta del Este");
        }

        private async Task ExecuteViewPodcastsCommand()
        {
            await ShowAlertAsync("Podcasts", "Próximamente: Biblioteca de podcasts y episodios");
        }

        private async Task ExecuteViewNewsCommand()
        {
            await ShowAlertAsync("Noticias", "Próximamente: Últimas noticias locales e internacionales");
        }

        private async Task ExecuteContactCommand()
        {
            await ShowAlertAsync("Contacto",
                "📻 Radio Punta del Este\n" +
                "📞 Tel: (598) 42 486 xxx\n" +
                "📧 info@radiopunta.com\n" +
                "📍 Punta del Este, Uruguay");
        }

        private async Task<bool> CheckConnectivityAsync()
        {
            try
            {
                var current = Connectivity.Current;
                var networkAccess = current.NetworkAccess;
                var isConnected = networkAccess == NetworkAccess.Internet;

                if (!isConnected)
                {
                    ConnectionStatus = "Sin conexión";
                    ConnectionStatusColor = Colors.Red;
                    ShowConnectionStatus = true;
                }
                else
                {
                    ConnectionStatus = "Conectado";
                    ConnectionStatusColor = Colors.Green;
                }

                return isConnected;
            }
            catch
            {
                ConnectionStatus = "Error de conexión";
                ConnectionStatusColor = Colors.Orange;
                return false;
            }
        }

        private async Task StartClockAsync()
        {
            try
            {
                while (true)
                {
                    CurrentTime = DateTime.Now.ToString("HH:mm");
                    await Task.Delay(60000); // Actualizar cada minuto
                }
            }
            catch (TaskCanceledException)
            {
                // Expected when cancelled
            }
        }

        private async Task LoadCurrentShowInfoAsync()
        {
            try
            {
                // Aquí cargarías la información del programa actual desde una API
                var hour = DateTime.Now.Hour;

                CurrentShow = hour switch
                {
                    >= 6 and < 10 => "Buenos Días Punta del Este",
                    >= 10 and < 14 => "Radio Mediodía",
                    >= 14 and < 18 => "Tarde Radial",
                    >= 18 and < 22 => "Noche en Punta",
                    _ => "Música Nocturna"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading show info: {ex.Message}");
                CurrentShow = "Radio en Vivo";
            }
        }

        private async Task ShowAlertAsync(string title, string message)
        {
            try
            {
                await Application.Current?.MainPage?.DisplayAlert(title, message, "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing alert: {ex.Message}");
            }
        }

        private void ShowToast(string message)
        {
            // Implementar toast notification
            System.Diagnostics.Debug.WriteLine($"Toast: {message}");
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
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