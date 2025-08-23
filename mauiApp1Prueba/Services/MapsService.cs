using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using System.Collections.Generic;
using System.Threading.Tasks;
using mauiApp1Prueba.Models;

namespace mauiApp1Prueba.Services
{
    public class MapsService
    {
        private readonly ISponsorService _sponsorService;
        private readonly IGeolocationService _geolocationService;

        public MapsService(ISponsorService sponsorService, IGeolocationService geolocationService)
        {
            _sponsorService = sponsorService;
            _geolocationService = geolocationService;
        }

        // Devuelve una lista de Pins para MAUI Maps
        public async Task<List<Pin>> GetSponsorPinsAsync()
        {
            var sponsors = await _sponsorService.GetAllSponsorsAsync();
            var pins = new List<Pin>();

            foreach (var sponsor in sponsors)
            {
                if (sponsor.Latitude != 0 && sponsor.Longitude != 0)
                {
                    pins.Add(new Pin
                    {
                        Label = sponsor.Name,                // Nombre del patrocinador
                        Address = sponsor.Address,           // Dirección (opcional)
                        Type = PinType.Place,
                        Location = new Location(sponsor.Latitude, sponsor.Longitude) // Lat/Lon correctos
                    });
                }
            }

            return pins;
        }

        // Devuelve la ubicación actual del usuario como Location de MAUI
        public async Task<Location?> GetCurrentLocationAsync()
        {
            try
            {
                var current = await _geolocationService.GetCurrentLocationAsync();

                if (current == null) return null;

                // Convertir a Microsoft.Maui.Devices.Sensors.Location
                return new Location(current.Latitude, current.Longitude);
            }
            catch
            {
                return null;
            }
        }
    }
}
