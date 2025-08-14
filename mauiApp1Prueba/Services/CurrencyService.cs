using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Maui.Storage;
using mauiApp1Prueba.Models;

namespace mauiApp1Prueba.Services
{
    public class CurrencyService
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = "da05f51aa5fa48102ba10b58210d8b57";
        private const string BaseUrl = "http://api.currencylayer.com/live";

        public CurrencyService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<ExchangeRates> GetRatesAsync(bool forceRefresh = false)
        {
            // Simplificado: siempre llama a la API (puedes agregar caché después)
            var url = $"{BaseUrl}?access_key={ApiKey}&currencies=UYU,EUR,BRL&source=USD&format=1";

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var resp = await _httpClient.GetFromJsonAsync<CurrencyLayerResponse>(url, options)
                          ?? throw new Exception("Respuesta vacía de CurrencyLayer.");

                if (!resp.Success)
                    throw new Exception($"CurrencyLayer error {resp.Error?.Code}: {resp.Error?.Info}");

                var q = resp.Quotes;
                if (!q.ContainsKey("USDUYU") || !q.ContainsKey("USDEUR") || !q.ContainsKey("USDBRL"))
                    throw new Exception("Faltan cotizaciones esperadas en la respuesta.");

                // Calcula frente a UYU
                var usdUyu = q["USDUYU"];
                var eurUyu = usdUyu / q["USDEUR"];
                var brlUyu = usdUyu / q["USDBRL"];
                var lastUtc = DateTimeOffset.FromUnixTimeSeconds(resp.Timestamp);
                var lastLocal = lastUtc.ToLocalTime();

                return new ExchangeRates(
                    decimal.Round(usdUyu, 4),
                    decimal.Round(eurUyu, 4),
                    decimal.Round(brlUyu, 4),
                    lastUtc,
                    lastLocal
                );
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error de conexión: {ex.Message}");
            }
            catch (JsonException ex)
            {
                throw new Exception($"Error al procesar respuesta de API: {ex.Message}");
            }
        }
    }
}