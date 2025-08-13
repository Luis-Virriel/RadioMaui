using mauiApp1Prueba.ViewModels;

namespace mauiApp1Prueba.Views;

public partial class LocationPickerPage : ContentPage
{
    private readonly LocationPickerViewModel _viewModel;

    public LocationPickerPage(LocationPickerViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // La página funciona sin mapa, usando solo las opciones de ubicación
    }
}