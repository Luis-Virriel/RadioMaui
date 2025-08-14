using mauiApp1Prueba.ViewModels;

namespace mauiApp1Prueba.Views;

public partial class PaginaCine : ContentPage
{
    public PaginaCine(PaginaCineViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Opcional: Refrescar datos cuando la página aparece
        if (BindingContext is PaginaCineViewModel viewModel)
        {
            // Solo cargar si no hay datos
            if (!viewModel.Movies.Any())
            {
                await Task.Delay(100); // Pequeño delay para que la UI se establezca
                viewModel.LoadMoviesCommand?.Execute(null);
            }
        }
    }
}