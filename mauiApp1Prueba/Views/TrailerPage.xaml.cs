using mauiApp1Prueba.ViewModels;
using mauiApp1Prueba.Models;

namespace mauiApp1Prueba.Views;

[QueryProperty(nameof(Movie), "Movie")]
public partial class TrailerPage : ContentPage
{
    private readonly TrailerPageViewModel _viewModel;
    private Movie _pendingMovie;

    public TrailerPage(TrailerPageViewModel viewModel)
    {
        try
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en constructor TrailerPage: {ex.Message}");
            // Si falla la inicialización, crear un ViewModel temporal
            _viewModel = viewModel;
        }
    }

    public Movie Movie
    {
        set
        {
            try
            {
                if (value != null)
                {
                    _pendingMovie = value;
                    System.Diagnostics.Debug.WriteLine($"Movie asignado: {value.Title}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error asignando Movie: {ex.Message}");
            }
        }
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();

            // Dar tiempo para que la UI se estabilice
            await Task.Delay(100);

            if (_pendingMovie != null && _viewModel != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inicializando ViewModel para: {_pendingMovie.Title}");
                await _viewModel.InitializeAsync(_pendingMovie);
                _pendingMovie = null; // Limpiar referencia
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en OnAppearing: {ex.Message}");

            // Mostrar error al usuario y volver atrás
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    await DisplayAlert("Error",
                        "No se pudo cargar la información del trailer. Volviendo a la lista de películas.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                catch
                {
                    // Si falla incluso el alert, intentar navegar atrás directamente
                    try
                    {
                        await Shell.Current.GoToAsync("..");
                    }
                    catch (Exception navEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error navegando atrás: {navEx.Message}");
                    }
                }
            });
        }
    }

    protected override void OnDisappearing()
    {
        try
        {
            base.OnDisappearing();
            _pendingMovie = null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en OnDisappearing: {ex.Message}");
        }
    }
}