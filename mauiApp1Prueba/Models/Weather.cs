using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiApp1Prueba.Models
{
    public class Weather
    {
        public string Description { get; set; }
        public string Icon { get; set; }
        public double Temperature { get; set; }
        public double FeelsLike { get; set; }
        public int Humidity { get; set; }
    }
}
