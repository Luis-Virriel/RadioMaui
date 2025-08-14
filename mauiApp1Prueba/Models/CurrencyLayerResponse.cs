using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiApp1Prueba.Models
{

    public class CurrencyLayerResponse
    {
        public bool Success { get; set; }
        public long Timestamp { get; set; }
        public string Source { get; set; } = "";
        public Dictionary<string, decimal> Quotes { get; set; } = new();
        public CurrencyLayerError? Error { get; set; }
    }

    public class CurrencyLayerError
    {
        public int Code { get; set; }
        public string Info { get; set; } = "";
    }
}
