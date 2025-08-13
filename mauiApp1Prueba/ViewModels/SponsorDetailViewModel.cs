using mauiApp1Prueba.Models;
using mauiApp1Prueba.Services;
using System.Windows.Input;

namespace mauiApp1Prueba.ViewModels
{
    public class SponsorDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly ISponsorService _sponsorService;
        private readonly IGeolocationService _geolocationService;
        private int _sponsorId;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private string _address = string.Empty;
        private string _logoPath = string.Empty;
        private double _latitude;
        private double _longitude;
        private bool _isEditMode;

        public int SponsorId
        {
            get => _sponsorId;
            set => SetProperty(ref _sponsorId, value);
        }

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    ((Command)SaveCommand).ChangeCanExecute();
                }
            }
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string Address
        {
            get => _address;
            set
            {
                if (SetProperty(ref _address, value))
                {
                    ((Command)SaveCommand).ChangeCanExecute();
                }
            }
        }

        public string LogoPath
        {
            get => _logoPath;
            set => SetProperty(ref _logoPath, value);
        }

        public double Latitude
        {
            get => _latitude;
            set => SetProperty(ref _latitude, value);
        }

        public double Longitude
        {
            get => _longitude;
            set => SetProperty(ref _longitude, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (SetProperty(ref _isEditMode, value))
                {
                    PageTitle = value ? "Editar Patrocinador" : "Nuevo Patrocinador";
                }
            }
        }

        // Commands
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand UseCurrentLocationCommand { get; }
        public ICommand SelectOnMapCommand { get; }
        public ICommand SelectImageCommand { get; }

        public SponsorDetailViewModel(ISponsorService sponsorService, IGeolocationService geolocationService)
        {
            _sponsorService = sponsorService;
            _geolocationService = geolocationService;
            PageTitle = "Nuevo Patrocinador";

            // Initialize commands
            SaveCommand = new Command(async () => await SaveSponsorAsync(), CanSave);
            CancelCommand = new Command(async () => await CancelAsync());
            UseCurrentLocationCommand = new Command(async () => await UseCurrentLocationAsync());
            SelectOnMapCommand = new Command(async () => await SelectOnMapAsync());
            SelectImageCommand = new Command(async () => await SelectImageAsync());
        }

        public async Task LoadSponsorAsync(int id)
        {
            if (id <= 0) return;

            try
            {
                SetBusyState(true, "Cargando patrocinador...");

                var sponsor = await _sponsorService.GetSponsorByIdAsync(id);
                if (sponsor != null)
                {
                    SponsorId = sponsor.Id;
                    Name = sponsor.Name;
                    Description = sponsor.Description ?? string.Empty;
                    Address = sponsor.Address;
                    LogoPath = sponsor.LogoPath ?? string.Empty;
                    Latitude = sponsor.Latitude;
                    Longitude = sponsor.Longitude;
                    IsEditMode = true;
                }
            }
            catch (Exception ex)
            {
                await Application.Current?.MainPage?.DisplayAlert("Error",
                    $"No se pudo cargar el patrocinador: {ex.Message}", "OK");
            }
            finally
            {
                SetBusyState(false);
            }
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   !string.IsNullOrWhiteSpace(Address) &&
                   !IsBusy;
        }

        private async Task SaveSponsorAsync()
        {
            if (!CanSave()) return;

            try
            {
                SetBusyState(true, IsEditMode ? "Actualizando..." : "Guardando...");

                var sponsor = new Sponsor
                {
                    Id = SponsorId,
                    Name = Name.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                    Address = Address.Trim(),
                    LogoPath = string.IsNullOrWhiteSpace(LogoPath) ? null : LogoPath,
                    Latitude = Latitude,
                    Longitude = Longitude
                };

                bool success;
                if (IsEditMode)
                {
                    success = await _sponsorService.UpdateSponsorAsync(sponsor);
                }
                else
                {
                    var result = await _sponsorService.AddSponsorAsync(sponsor);
                    success = result > 0;
                }

                if (success)
                {
                    await Application.Current?.MainPage?.DisplayAlert("Éxito",
                        IsEditMode ? "Patrocinador actualizado" : "Patrocinador guardado", "OK");

                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Application.Current?.MainPage?.DisplayAlert("Error",
                        "No se pudo guardar el patrocinador", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current?.MainPage?.DisplayAlert("Error",
                    $"Error al guardar: {ex.Message}", "OK");
            }
            finally
            {
                SetBusyState(false);
            }
        }

        private async Task CancelAsync()
        {
            var hasChanges = !string.IsNullOrWhiteSpace(Name) ||
                           !string.IsNullOrWhiteSpace(Description) ||
                           !string.IsNullOrWhiteSpace(Address);

            if (hasChanges)
            {
                var result = await Application.Current?.MainPage?.DisplayAlert("Confirmar",
                    "¿Descartar los cambios?", "Sí", "No");

                if (result != true) return;
            }

            await Shell.Current.GoToAsync("..");
        }

        private async Task UseCurrentLocationAsync()
        {
            try
            {
                SetBusyState(true, "Obteniendo ubicación actual...");

                var location = await _geolocationService.GetCurrentLocationAsync();

                if (location != null)
                {
                    Latitude = location.Latitude;
                    Longitude = location.Longitude;

                    // Obtener dirección de las coordenadas
                    var address = await _geolocationService.GetAddressFromCoordinatesAsync(location.Latitude, location.Longitude);
                    if (!string.IsNullOrEmpty(address))
                    {
                        Address = address;
                    }

                    await Application.Current?.MainPage?.DisplayAlert("Ubicación obtenida",
                        $"Se estableció tu ubicación actual", "OK");
                }
                else
                {
                    await Application.Current?.MainPage?.DisplayAlert("Error",
                        "No se pudo obtener la ubicación. Verifica que el GPS esté habilitado y los permisos estén concedidos.", "OK");
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

        private async Task SelectOnMapAsync()
        {
            try
            {
                // Navegar al selector de mapa
                var navigationParameter = new Dictionary<string, object>
                {
                    ["lat"] = Latitude.ToString(),
                    ["lon"] = Longitude.ToString()
                };

                await Shell.Current.GoToAsync("locationpicker", navigationParameter);
            }
            catch (Exception ex)
            {
                await Application.Current?.MainPage?.DisplayAlert("Error",
                    $"Error al abrir el selector de ubicación: {ex.Message}", "OK");
            }
        }

        // Método para recibir datos del selector de ubicación
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("latitude") && query.ContainsKey("longitude") && query.ContainsKey("address"))
            {
                if (double.TryParse(query["latitude"].ToString(), out double lat) &&
                    double.TryParse(query["longitude"].ToString(), out double lon))
                {
                    Latitude = lat;
                    Longitude = lon;
                    Address = query["address"].ToString() ?? "";
                }
            }
        }

        private async Task SelectImageAsync()
        {
            try
            {
                // Por ahora, permitir ingresar URL de imagen
                // TODO: Implementar selector de imagen desde galería
                var result = await Application.Current?.MainPage?.DisplayPromptAsync("Logo",
                    "Ingresa la URL de la imagen o ruta:", initialValue: LogoPath);

                if (!string.IsNullOrWhiteSpace(result))
                {
                    LogoPath = result;
                }
            }
            catch (Exception ex)
            {
                await Application.Current?.MainPage?.DisplayAlert("Error",
                    $"Error al seleccionar imagen: {ex.Message}", "OK");
            }
        }
    }
}