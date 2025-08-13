using mauiApp1Prueba.ViewModels;

namespace mauiApp1Prueba.Views;

public partial class PaginaPatrocinadores : ContentPage
{
    private readonly PatrocinadoresViewModel _viewModel;

    public PaginaPatrocinadores(PatrocinadoresViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel != null)
        {
            await _viewModel.InitializeAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Limpiar selección si es necesario
        if (_viewModel != null)
        {
            _viewModel.SelectedSponsor = null;
        }
    }
}