using mauiApp1Prueba.ViewModels;

namespace mauiApp1Prueba.Views;

public partial class PaginaNoticias : ContentPage
{
    public PaginaNoticias()
    {
        InitializeComponent();
        BindingContext = new NewsViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var viewModel = (NewsViewModel)BindingContext;
        if (viewModel.Items.Count == 0)
        {
            await viewModel.LoadAsync();
        }
    }
}
