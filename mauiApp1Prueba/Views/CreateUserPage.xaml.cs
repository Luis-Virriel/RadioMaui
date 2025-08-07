using mauiApp1Prueba.ViewModels;

namespace mauiApp1Prueba.Views;

public partial class CreateUserPage : ContentPage
{
    private CreateUserViewModel _viewModel;

    public CreateUserPage(CreateUserViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // Suscribirse a eventos del ViewModel si es necesario
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CreateUserViewModel.SuccessMessage) &&
            !string.IsNullOrEmpty(_viewModel.SuccessMessage))
        {
            await DisplayAlert("¡Éxito!", _viewModel.SuccessMessage, "OK");
            await Navigation.PopAsync(); // Vuelve a LoginPage que está en la pila
        }
    }


    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_viewModel != null)
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }
}