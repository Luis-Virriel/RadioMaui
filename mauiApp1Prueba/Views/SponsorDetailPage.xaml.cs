using mauiApp1Prueba.ViewModels;

namespace mauiApp1Prueba.Views;

[QueryProperty(nameof(SponsorId), "id")]
public partial class SponsorDetailPage : ContentPage
{
    private readonly SponsorDetailViewModel _viewModel;

    public string SponsorId
    {
        set
        {
            if (_viewModel != null && int.TryParse(value, out int id) && id > 0)
            {
                Task.Run(async () => await _viewModel.LoadSponsorAsync(id));
            }
        }
    }

    public SponsorDetailPage(SponsorDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Si no hay ID, es un nuevo patrocinador
        if (_viewModel != null && _viewModel.SponsorId == 0)
        {
            _viewModel.IsEditMode = false;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Limpiar el formulario si es necesario
        // (Se puede implementar lógica adicional aquí)
    }
}