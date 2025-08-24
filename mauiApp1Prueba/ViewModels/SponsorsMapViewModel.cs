using Microsoft.Maui.Controls.Maps;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using mauiApp1Prueba.Services;

namespace mauiApp1Prueba.ViewModels
{
    public class SponsorsMapViewModel : BaseViewModel
    {
        private readonly MapsService _mapsService;

        public ObservableCollection<Pin> SponsorPins { get; } = new();

        public SponsorsMapViewModel(MapsService mapsService)
        {
            _mapsService = mapsService;
        }

        public async Task LoadMapAsync()
        {
            if (IsBusy) return;
            SetBusyState(true, "Cargando mapa...");

            try
            {
                var pins = await _mapsService.GetSponsorPinsAsync();

                SponsorPins.Clear();
                foreach (var pin in pins)
                    SponsorPins.Add(pin);

                var currentLocation = await _mapsService.GetCurrentLocationAsync();
                if (currentLocation != null)
                {
                    // Aquí podrías centrar el mapa en la ubicación actual
                }
            }
            catch (System.Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    $"No se pudo cargar el mapa: {ex.Message}", "OK");
            }
            finally
            {
                SetBusyState(false);
            }
        }
    }
}
