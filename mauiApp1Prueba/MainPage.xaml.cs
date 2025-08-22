using mauiApp1Prueba.ViewModels;
using Microsoft.Maui.Storage;

namespace mauiApp1Prueba;

public partial class MainPage : ContentPage
{
    private readonly RadioHomeViewModel _viewModel;
    private CancellationTokenSource _rotationAnimationCancellation = new();
    private bool _isRotating = false;

    public MainPage(RadioHomeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;

        // Suscribirse a cambios en las propiedades para manejar animaciones
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ConfigurarVisibilidadSecciones();
    }

    private void ConfigurarVisibilidadSecciones()
    {
        // Configurar visibilidad basada en preferencias
        NoticiasFrame.IsVisible = Preferences.Get("MostrarNoticias", true);
        CineFrame.IsVisible = Preferences.Get("MostrarCine", true);
        ClimaFrame.IsVisible = Preferences.Get("MostrarClima", true);
        CotizacionesFrame.IsVisible = Preferences.Get("MostrarCotizaciones", true);
        PatrocinadoresFrame.IsVisible = Preferences.Get("MostrarPatrocinadores", true);

        // Reorganizar el grid para eliminar espacios vacíos
        ReorganizarGrid();
    }

    private void ReorganizarGrid()
    {
        var sectionsGrid = this.FindByName<Grid>("SectionsGrid");
        if (sectionsGrid == null) return;

        // Limpiar el grid
        sectionsGrid.Children.Clear();
        sectionsGrid.RowDefinitions.Clear();

        var visibleFrames = new List<Frame>();

        if (NoticiasFrame.IsVisible) visibleFrames.Add(NoticiasFrame);
        if (CineFrame.IsVisible) visibleFrames.Add(CineFrame);
        if (ClimaFrame.IsVisible) visibleFrames.Add(ClimaFrame);
        if (CotizacionesFrame.IsVisible) visibleFrames.Add(CotizacionesFrame);
        if (PatrocinadoresFrame.IsVisible) visibleFrames.Add(PatrocinadoresFrame);

        // Calcular filas necesarias
        int rows = (int)Math.Ceiling(visibleFrames.Count / 2.0);
        if (PatrocinadoresFrame.IsVisible) rows++; // Patrocinadores ocupa una fila completa

        // Agregar definiciones de fila
        for (int i = 0; i < rows; i++)
        {
            sectionsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        // Colocar frames en el grid
        int currentRow = 0;
        int currentColumn = 0;

        foreach (var frame in visibleFrames)
        {
            if (frame == PatrocinadoresFrame)
            {
                // Patrocinadores ocupa toda la fila
                Grid.SetRow(frame, currentRow);
                Grid.SetColumn(frame, 0);
                Grid.SetColumnSpan(frame, 2);
                currentRow++;
                currentColumn = 0;
            }
            else
            {
                Grid.SetRow(frame, currentRow);
                Grid.SetColumn(frame, currentColumn);
                Grid.SetColumnSpan(frame, 1);

                currentColumn++;
                if (currentColumn >= 2)
                {
                    currentColumn = 0;
                    currentRow++;
                }
            }

            sectionsGrid.Children.Add(frame);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopAllAnimations();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RadioHomeViewModel.IsPlaying))
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_viewModel.IsPlaying)
                {
                    StartPlayIconRotation();
                }
                else
                {
                    StopPlayIconRotation();
                }
            });
        }
    }

    private async void StartPlayIconRotation()
    {
        if (_isRotating) return;

        _isRotating = true;
        _rotationAnimationCancellation?.Cancel();
        _rotationAnimationCancellation = new CancellationTokenSource();

        try
        {
            while (!_rotationAnimationCancellation.Token.IsCancellationRequested && _viewModel.IsPlaying)
            {
                await PlayIcon.RotateTo(PlayIcon.Rotation + 360, 8000, Easing.Linear);

                if (_rotationAnimationCancellation.Token.IsCancellationRequested) break;
            }
        }
        catch (TaskCanceledException)
        {
            // Expected when animation is cancelled
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in play icon rotation: {ex.Message}");
        }
        finally
        {
            _isRotating = false;
        }
    }

    private async void StopPlayIconRotation()
    {
        _rotationAnimationCancellation?.Cancel();

        try
        {
            if (_isRotating)
            {
                await PlayIcon.RotateTo(0, 1000, Easing.CubicOut);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error stopping play icon rotation: {ex.Message}");
        }
    }

    private void StopAllAnimations()
    {
        _rotationAnimationCancellation?.Cancel();
        _isRotating = false;
    }

    // Navegación a las diferentes páginas
    private async void OnNoticiasClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("//PaginaNoticias");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo navegar a Noticias: {ex.Message}", "OK");
        }
    }

    private async void OnCineClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("//PaginaCine");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo navegar a Cine: {ex.Message}", "OK");
        }
    }

    private async void OnClimaClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("//PaginaClima");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo navegar a Clima: {ex.Message}", "OK");
        }
    }

    private async void OnCotizacionesClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("//PaginaCotizaciones");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo navegar a Cotizaciones: {ex.Message}", "OK");
        }
    }

    private async void OnPatrocinadoresClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("//patrocinadores");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo navegar a Patrocinadores: {ex.Message}", "OK");
        }
    }
}