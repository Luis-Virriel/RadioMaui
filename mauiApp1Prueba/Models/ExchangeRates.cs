using System;

namespace mauiApp1Prueba.Models
{
    public record ExchangeRates
    (
    decimal UsdUyu,
    decimal EurUyu,
    decimal BrlUyu,
    DateTimeOffset LastUpdatedUtc,
    DateTimeOffset LastUpdatedLocal
    );
}
