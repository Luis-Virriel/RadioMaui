using System.Globalization;
using mauiApp1Prueba.Models;
using mauiApp1Prueba.Services;
using System.Collections.ObjectModel;
using Microsoft.Maui.Storage;

namespace mauiApp1Prueba.Views;

public partial class PaginaClima : ContentPage
{
    private readonly WeatherServices _weatherServices;
    public ObservableCollection<ForecastItemViewModel> ForecastItems { get; set; }

    private const string LastUpdateKey = "LastWeatherUpdate";
    private const string WeatherCacheKey = "CachedWeather";
    private const string ForecastCacheKey = "CachedForecast";

    public PaginaClima()
    {
        InitializeComponent();
        _weatherServices = new WeatherServices();
        ForecastItems = new ObservableCollection<ForecastItemViewModel>();
        ForecastCollection.ItemsSource = ForecastItems;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var (cachedWeather, cachedForecast) = LoadWeatherCache();
        if (cachedWeather != null && cachedForecast != null)
            DisplayWeather(cachedWeather, cachedForecast);

        var lastUpdate = Preferences.Get(LastUpdateKey, DateTime.MinValue);
        if (lastUpdate.Date < DateTime.Now.Date)
            await LoadWeatherAndSaveCacheAsync();
        else
            LblLastUpdate.Text = $"Última actualización: {lastUpdate:dd/MM/yyyy HH:mm}";
    }

    private async Task LoadWeatherAndSaveCacheAsync()
    {
        try
        {
            Weather current = await _weatherServices.GetCurrentWeatherAsync();
            List<ForecastResponse> forecast = await _weatherServices.Get5DayForecastAsync();

            Preferences.Set(WeatherCacheKey, System.Text.Json.JsonSerializer.Serialize(current));
            Preferences.Set(ForecastCacheKey, System.Text.Json.JsonSerializer.Serialize(forecast));
            Preferences.Set(LastUpdateKey, DateTime.Now);

            DisplayWeather(current, forecast);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo obtener el clima: {ex.Message}", "OK");
        }
    }

    private (Weather, List<ForecastResponse>) LoadWeatherCache()
    {
        var weatherJson = Preferences.Get(WeatherCacheKey, null);
        var forecastJson = Preferences.Get(ForecastCacheKey, null);

        if (!string.IsNullOrEmpty(weatherJson) && !string.IsNullOrEmpty(forecastJson))
        {
            var weather = System.Text.Json.JsonSerializer.Deserialize<Weather>(weatherJson);
            var forecast = System.Text.Json.JsonSerializer.Deserialize<List<ForecastResponse>>(forecastJson);
            return (weather, forecast);
        }

        return (null, null);
    }

    private void DisplayWeather(Weather current, List<ForecastResponse> forecast)
    {
        LblLocation.Text = "Punta del Este, Uruguay";
        LblTemp.Text = $"{Math.Round(current.Temperature)} °C";
        LblDesc.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(current.Description);
        ImgIcon.Source = $"https://openweathermap.org/img/wn/{current.Icon}@2x.png";
        LblLastUpdate.Text = $"Última actualización: {DateTime.Now:dd/MM/yyyy HH:mm}";

        var noonForecasts = forecast
            .GroupBy(f => f.DateTime.Date)
            .Select(g => new ForecastItemViewModel(g.FirstOrDefault(f => f.DateTime.Hour == 12) ?? g.First()))
            .Take(5)
            .ToList();

        ForecastItems.Clear();
        foreach (var item in noonForecasts)
            ForecastItems.Add(item);
    }

    private async void BtnRefresh_Clicked(object sender, EventArgs e)
    {
        await LoadWeatherAndSaveCacheAsync();
    }
}

// ViewModel para pronóstico
public class ForecastItemViewModel
{
    public string Date { get; }
    public string IconUrl { get; }
    public string TempDisplay { get; }
    public string Description { get; }

    public ForecastItemViewModel(ForecastResponse forecast)
    {
        Date = forecast.DateTime.ToString("ddd dd/MM", new CultureInfo("es-ES"));
        IconUrl = $"https://openweathermap.org/img/wn/{forecast.Icon}@2x.png";
        TempDisplay = $"{Math.Round(forecast.Temperature)} °C";
        Description = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(forecast.Description);
    }
}
