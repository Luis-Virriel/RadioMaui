namespace mauiApp1Prueba.Models
{
    public class SponsorLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Accuracy { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public SponsorLocation() { }

        public SponsorLocation(double latitude, double longitude, double? accuracy = null)
        {
            Latitude = latitude;
            Longitude = longitude;
            Accuracy = accuracy;
        }

        /// <summary>
        /// Calcula la distancia en kilómetros entre dos ubicaciones usando la fórmula de Haversine
        /// </summary>
        public double DistanceTo(SponsorLocation other)
        {
            return DistanceTo(other.Latitude, other.Longitude);
        }

        /// <summary>
        /// Calcula la distancia en kilómetros a una coordenada específica
        /// </summary>
        public double DistanceTo(double latitude, double longitude)
        {
            const double earthRadius = 6371; // Radio de la Tierra en kilómetros

            var lat1Rad = ToRadians(Latitude);
            var lat2Rad = ToRadians(latitude);
            var deltaLatRad = ToRadians(latitude - Latitude);
            var deltaLonRad = ToRadians(longitude - Longitude);

            var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadius * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public override string ToString()
        {
            return $"{Latitude:F6}, {Longitude:F6}";
        }
    }
}