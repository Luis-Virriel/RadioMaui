using mauiApp1Prueba.Models;
using mauiApp1Prueba.ViewModels;

namespace mauiApp1Prueba.Views;

[QueryProperty(nameof(Movie), "Movie")]
public partial class TrailerPage : ContentPage, IQueryAttributable
{
    private readonly TrailerPageViewModel _viewModel;
    private Movie _movie;

    public Movie Movie
    {
        get => _movie;
        set
        {
            _movie = value;
            if (_viewModel != null && value != null)
            {
                _ = Task.Run(async () => await _viewModel.InitializeAsync(value));
            }
        }
    }

    public TrailerPage(TrailerPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        try
        {
            if (query != null && query.ContainsKey("Movie"))
            {
                Movie = query["Movie"] as Movie;
                System.Diagnostics.Debug.WriteLine($"TrailerPage recibió película: {Movie?.Title}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en ApplyQueryAttributes: {ex.Message}");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Asegurar que el WebView esté listo si hay un trailer
        if (_viewModel?.HasTrailer == true && !string.IsNullOrEmpty(_viewModel.TrailerEmbedUrl))
        {
            // Forzar la actualización del WebView si es necesario
            TrailerWebView.Source = _viewModel.TrailerEmbedUrl;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Limpiar el WebView al salir para liberar memoria
        try
        {
            if (TrailerWebView != null)
            {
                TrailerWebView.Source = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error limpiando WebView: {ex.Message}");
        }
    }
}