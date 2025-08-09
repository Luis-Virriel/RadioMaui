using System.Globalization;
using mauiApp1Prueba.Models;
using mauiApp1Prueba.Services;
using System.Collections.ObjectModel;

namespace mauiApp1Prueba.Views;

public partial class PaginaClima : ContentPage
{
    private readonly WeatherServices _weatherServices;
    public ObservableCollection<ForecastItemViewModel> ForecastItems { get; set; }

    public PaginaClima()
    {
        InitializeComponent();
        _weatherServices = new WeatherServices();
        ForecastItems = new ObservableCollection<ForecastItemViewModel>();
        ForecastCollection.ItemsSource = ForecastItems;
        LoadWeatherAsync();
    }

    private async void LoadWeatherAsync()
    {
        try
        {
            Weather current = await _weatherServices.GetCurrentWeatherAsync();

            LblLocation.Text = "Punta del Este, Uruguay";
            LblTemp.Text = $"{Math.Round(current.Temperature)} °C";
            LblDesc.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(current.Description);
            ImgIcon.Source = $"https://openweathermap.org/img/wn/{current.Icon}@2x.png";

            List<ForecastResponse> forecast = await _weatherServices.Get5DayForecastAsync();

            // Filtrar un ítem por día a las 12:00h
            var noonForecasts = forecast
                .Where(f => f.DateTime.Hour == 12)
                .GroupBy(f => f.DateTime.Date)
                .Select(g => g.First())
                .Take(5)
                .Select(f => new ForecastItemViewModel(f))
                .ToList();

            ForecastItems.Clear();
            foreach (var item in noonForecasts)
            {
                ForecastItems.Add(item);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo obtener el clima: {ex.Message}", "OK");
        }
    }
}

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
