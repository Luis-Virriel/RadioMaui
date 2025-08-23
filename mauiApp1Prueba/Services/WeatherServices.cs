using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using mauiApp1Prueba.Models;

namespace mauiApp1Prueba.Services
{
    public class WeatherServices
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = "40ed30cc4da04b0fed28c1dd01a0e483";
        private const string CityQuery = "Punta%20del%20Este,UY";

        public WeatherServices()
        {
            _httpClient = new HttpClient();
        }

        public async Task<Weather> GetCurrentWeatherAsync()
        {
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={CityQuery}&units=metric&lang=es&appid={ApiKey}";
            var response = await _httpClient.GetStringAsync(url);

            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;
            var weatherElement = root.GetProperty("weather")[0];
            var main = root.GetProperty("main");

            return new Weather
            {
                Description = weatherElement.GetProperty("description").GetString(),
                Icon = weatherElement.GetProperty("icon").GetString(),
                Temperature = main.GetProperty("temp").GetDouble(),
                FeelsLike = main.GetProperty("feels_like").GetDouble(),
                Humidity = main.GetProperty("humidity").GetInt32()
            };
        }

        public async Task<List<ForecastResponse>> Get5DayForecastAsync()
        {
            var url = $"https://api.openweathermap.org/data/2.5/forecast?q={CityQuery}&units=metric&lang=es&appid={ApiKey}";
            var response = await _httpClient.GetStringAsync(url);

            using var doc = JsonDocument.Parse(response);
            var list = doc.RootElement.GetProperty("list");

            var forecastList = new List<ForecastResponse>();
            foreach (var item in list.EnumerateArray())
            {
                var weatherElement = item.GetProperty("weather")[0];
                var main = item.GetProperty("main");
                var dt = item.GetProperty("dt_txt").GetString();

                forecastList.Add(new ForecastResponse
                {
                    DateTime = DateTime.Parse(dt),
                    Description = weatherElement.GetProperty("description").GetString(),
                    Icon = weatherElement.GetProperty("icon").GetString(),
                    Temperature = main.GetProperty("temp").GetDouble()
                });
            }

            return forecastList;
        }
    }
}
