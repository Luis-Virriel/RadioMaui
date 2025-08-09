using mauiApp1Prueba.ViewModels;

namespace mauiApp1Prueba;

public partial class MainPage : ContentPage
{
    private readonly RadioHomeViewModel _viewModel;
    private CancellationTokenSource _liveAnimationCancellation = new();
    private CancellationTokenSource _rotationAnimationCancellation = new();
    private bool _isRotating = false;
    private bool _isBlinking = false;

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
        StartLiveIndicatorAnimation();
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
                    StartRadioIconRotation();
                }
                else
                {
                    StopRadioIconRotation();
                }
            });
        }
    }

    private async void StartLiveIndicatorAnimation()
    {
        if (_isBlinking) return;

        _isBlinking = true;

        try
        {
            while (!_liveAnimationCancellation.Token.IsCancellationRequested && _viewModel.IsLive)
            {
                await LiveIndicator.FadeTo(0.3, 1000, Easing.Linear);
                if (_liveAnimationCancellation.Token.IsCancellationRequested) break;

                await LiveIndicator.FadeTo(1.0, 1000, Easing.Linear);
                if (_liveAnimationCancellation.Token.IsCancellationRequested) break;
            }
        }
        catch (TaskCanceledException)
        {
            // Expected when animation is cancelled
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in live indicator animation: {ex.Message}");
        }
        finally
        {
            _isBlinking = false;
        }
    }

    private async void StartRadioIconRotation()
    {
        if (_isRotating) return;

        _isRotating = true;
        _rotationAnimationCancellation?.Cancel();
        _rotationAnimationCancellation = new CancellationTokenSource();

        try
        {
            while (!_rotationAnimationCancellation.Token.IsCancellationRequested && _viewModel.IsPlaying)
            {
                await RadioIcon.RotateTo(RadioIcon.Rotation + 360, 10000, Easing.Linear);

                if (_rotationAnimationCancellation.Token.IsCancellationRequested) break;
            }
        }
        catch (TaskCanceledException)
        {
            // Expected when animation is cancelled
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in radio icon rotation: {ex.Message}");
        }
        finally
        {
            _isRotating = false;
        }
    }

    private async void StopRadioIconRotation()
    {
        _rotationAnimationCancellation?.Cancel();

        try
        {
            if (_isRotating)
            {
                await RadioIcon.RotateTo(0, 1000, Easing.CubicOut);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error stopping radio icon rotation: {ex.Message}");
        }
    }

    private void StopAllAnimations()
    {
        _liveAnimationCancellation?.Cancel();
        _rotationAnimationCancellation?.Cancel();

        _isBlinking = false;
        _isRotating = false;
    }
}