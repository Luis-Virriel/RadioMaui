using mauiApp1Prueba.Services;
using System.Windows.Input;

namespace mauiApp1Prueba.ViewModels
{
    [QueryProperty(nameof(InitialLatitude), "lat")]
    [QueryProperty(nameof(InitialLongitude), "lon")]
    public class LocationPickerViewModel : BaseViewModel
    {
        private readonly IGeolocationService _geolocationService;
        private double _selectedLatitude;
        private double _selectedLongitude;
        private string _selectedAddress = "Selecciona una ubicación en el mapa";

        public double SelectedLatitude
        {
            get => _selectedLatitude;
            set => SetProperty(ref _selectedLatitude, value);
        }

        public double SelectedLongitude
        {
            get => _selectedLongitude;
            set => SetProperty(ref _selectedLongitude, value);
        }

        public string SelectedAddress
        {
            get => _selectedAddress;
            set => SetProperty(ref _selectedAddress, value);
        }

        public string InitialLatitude
        {
            set
            {
                if (double.TryParse(value, out double lat))
                {
                    SelectedLatitude = lat;
                }
            }
        }

        public string InitialLongitude
        {
            set
            {
                if (double.TryParse(value, out double lon))
                {
                    SelectedLongitude = lon;
                }
            }
        }

        // Commands
        public ICommand GoToCurrentLocationCommand { get; }
        public ICommand ConfirmLocationCommand { get; }
        public ICommand CancelCommand { get; }

        public LocationPickerViewModel(IGeolocationService geolocationService)
        {
            _geolocationService = geolocationService;
            PageTitle = "Seleccionar Ubicación";

            // Ubicación por defecto (Montevideo)
            SelectedLatitude = -34.6118;
            SelectedLongitude = -56.1925;

            // Initialize commands
            GoToCurrentLocationCommand = new Command(async () => await GoToCurrentLocationAsync());
            ConfirmLocationCommand = new Command(async () => await ConfirmLocationAsync());
            CancelCommand = new Command(async () => await CancelAsync());
        }

        private async Task GoToCurrentLocationAsync()
        {
            try
            {
                SetBusyState(true, "Obteniendo ubicación actual...");

                var location = await _geolocationService.GetCurrentLocationAsync();
                if (location != null)
                {
                    SelectedLatitude = location.Latitude;
                    SelectedLongitude = location.Longitude;

                    // Obtener dirección
                    await UpdateAddressAsync();

                    // Notificar a la página para centrar el mapa
                    MessagingCenter.Send(this, "CenterMap", new { Latitude = SelectedLatitude, Longitude = SelectedLongitude });
                }
                else
                {
                    await Application.Current?.MainPage?.DisplayAlert("Error",
                        "No se pudo obtener la ubicación actual", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current?.MainPage?.DisplayAlert("Error",
                    $"Error al obtener ubicación: {ex.Message}", "OK");
            }
            finally
            {
                SetBusyState(false);
            }
        }

        private async Task ConfirmLocationAsync()
        {
            var result = new Dictionary<string, object>
            {
                ["latitude"] = SelectedLatitude,
                ["longitude"] = SelectedLongitude,
                ["address"] = SelectedAddress
            };

            await Shell.Current.GoToAsync("..", result);
        }

        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        public async Task OnMapTappedAsync(double latitude, double longitude)
        {
            SelectedLatitude = latitude;
            SelectedLongitude = longitude;
            await UpdateAddressAsync();
        }

        private async Task UpdateAddressAsync()
        {
            try
            {
                var address = await _geolocationService.GetAddressFromCoordinatesAsync(SelectedLatitude, SelectedLongitude);
                SelectedAddress = address ?? "Dirección no disponible";
            }
            catch (Exception ex)
            {
                SelectedAddress = "Error al obtener dirección";
                System.Diagnostics.Debug.WriteLine($"Error getting address: {ex.Message}");
            }
        }
    }
}