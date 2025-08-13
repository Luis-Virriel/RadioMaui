using mauiApp1Prueba.Models;

namespace mauiApp1Prueba.Services
{
    public interface IGeolocationService
    {
        Task<SponsorLocation?> GetCurrentLocationAsync();
        Task<string?> GetAddressFromCoordinatesAsync(double latitude, double longitude);
    }

    public class GeolocationService : IGeolocationService
    {
        public async Task<SponsorLocation?> GetCurrentLocationAsync()
        {
            try
            {
                // Verificar permisos
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (status != PermissionStatus.Granted)
                    {
                        return null;
                    }
                }

                // Obtener ubicación actual
                var request = new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(10)
                };

                var location = await Geolocation.Default.GetLocationAsync(request);

                if (location != null)
                {
                    return new SponsorLocation(location.Latitude, location.Longitude, location.Accuracy);
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting location: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> GetAddressFromCoordinatesAsync(double latitude, double longitude)
        {
            try
            {
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(latitude, longitude);
                var placemark = placemarks?.FirstOrDefault();

                if (placemark != null)
                {
                    // Construir dirección a partir de los datos disponibles
                    var addressParts = new List<string>();

                    if (!string.IsNullOrEmpty(placemark.Thoroughfare))
                        addressParts.Add(placemark.Thoroughfare);

                    if (!string.IsNullOrEmpty(placemark.SubThoroughfare))
                        addressParts.Add(placemark.SubThoroughfare);

                    if (!string.IsNullOrEmpty(placemark.Locality))
                        addressParts.Add(placemark.Locality);

                    if (!string.IsNullOrEmpty(placemark.AdminArea))
                        addressParts.Add(placemark.AdminArea);

                    return addressParts.Any() ? string.Join(", ", addressParts) : "Dirección no disponible";
                }

                return "Dirección no encontrada";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting address: {ex.Message}");
                return "Error al obtener dirección";
            }
        }
    }
}