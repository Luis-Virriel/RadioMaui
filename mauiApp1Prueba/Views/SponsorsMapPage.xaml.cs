using Microsoft.Maui.Controls.Maps;
using mauiApp1Prueba.Services;
using mauiApp1Prueba.ViewModels;

namespace mauiApp1Prueba.Views;

public partial class SponsorsMapPage : ContentPage
{
    private readonly SponsorsMapViewModel _viewModel;

    public SponsorsMapPage()
    {
        InitializeComponent();

        var sponsorService = new SponsorService();
        var geolocationService = new GeolocationService();
        var mapsService = new MapsService(sponsorService, geolocationService);

        _viewModel = new SponsorsMapViewModel(mapsService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadMapAsync();

        MapControl.Pins.Clear();
        foreach (var pin in _viewModel.SponsorPins)
            MapControl.Pins.Add(pin);
    }
}
