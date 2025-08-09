using System.Text.Json.Serialization;

namespace mauiApp1Prueba.Models
{
    public class ForecastResponse
    {
        public DateTime DateTime { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public double Temperature { get; set; }
    }
}
