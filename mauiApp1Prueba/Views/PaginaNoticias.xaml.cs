using mauiApp1Prueba.ViewModels;

namespace mauiApp1Prueba.Views;

public partial class PaginaNoticias : ContentPage
{
    public PaginaNoticias(NewsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Cargar datos cuando aparezca la página por primera vez
        var viewModel = (NewsViewModel)BindingContext;
        if (viewModel.Items.Count == 0)
        {
            await viewModel.LoadAsync();
        }
    }

    private async void OnCategoryChanged(object sender, EventArgs e)
    {
        // Recargar cuando cambie la categoría
        var viewModel = (NewsViewModel)BindingContext;
        await viewModel.LoadAsync();
    }
}