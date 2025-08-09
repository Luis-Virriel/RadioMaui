using Microsoft.Maui.Controls;

namespace mauiApp1Prueba.Services
{
    public interface IAudioService
    {
        Task<bool> PlayStreamAsync(string url);
        Task StopAsync();
        Task SetVolumeAsync(double volume);
        bool IsPlaying { get; }
        event EventHandler<bool> PlayingStateChanged;
        event EventHandler<string> ErrorOccurred;
    }

    public class AudioService : IAudioService
    {
        private bool _isPlaying = false;
        private double _currentVolume = 0.5;
        private string _currentUrl = string.Empty;
        private WebView? _audioWebView;

        public bool IsPlaying => _isPlaying;

        public event EventHandler<bool>? PlayingStateChanged;
        public event EventHandler<string>? ErrorOccurred;

        public async Task<bool> PlayStreamAsync(string url)
        {
            try
            {
                if (_isPlaying && _currentUrl == url)
                {
                    return true;
                }

                if (_isPlaying)
                {
                    await StopAsync();
                }

                _currentUrl = url;

                // Crear WebView para reproducir audio
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    _audioWebView = new WebView
                    {
                        IsVisible = false,
                        WidthRequest = 1,
                        HeightRequest = 1
                    };

                    // HTML que reproduce el stream de audio
                    var htmlContent = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1'>
                        <title>Radio Player</title>
                    </head>
                    <body>
                        <audio id='radioPlayer' controls autoplay style='width:100%'>
                            <source src='{url}' type='audio/mpeg'>
                            <source src='{url}' type='audio/ogg'>
                            <source src='{url}' type='audio/wav'>
                            Tu navegador no soporta audio HTML5.
                        </audio>
                        
                        <script>
                            const audio = document.getElementById('radioPlayer');
                            
                            audio.addEventListener('loadstart', function() {{
                                console.log('Cargando stream...');
                            }});
                            
                            audio.addEventListener('loadeddata', function() {{
                                console.log('Stream cargado, reproduciendo...');
                            }});
                            
                            audio.addEventListener('playing', function() {{
                                console.log('Reproduciendo stream');
                            }});
                            
                            audio.addEventListener('error', function(e) {{
                                console.error('Error reproduciendo stream:', e);
                            }});
                            
                            // Configurar volumen
                            audio.volume = {_currentVolume};
                            
                            // Intentar reproducir automáticamente
                            audio.play().catch(function(error) {{
                                console.error('Error auto-reproduciendo:', error);
                            }});
                        </script>
                    </body>
                    </html>";

                    var htmlSource = new HtmlWebViewSource
                    {
                        Html = htmlContent
                    };

                    _audioWebView.Source = htmlSource;

                    // Agregar a la página actual (invisible)
                    if (Application.Current?.MainPage is ContentPage currentPage)
                    {
                        if (currentPage.Content is Grid grid)
                        {
                            grid.Children.Add(_audioWebView);
                        }
                        else if (currentPage.Content is StackLayout stack)
                        {
                            stack.Children.Add(_audioWebView);
                        }
                    }
                });

                // Simular tiempo de carga
                await Task.Delay(2000);

                _isPlaying = true;
                PlayingStateChanged?.Invoke(this, true);

                System.Diagnostics.Debug.WriteLine($"🎵 Reproduciendo stream: {url}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error reproduciendo audio: {ex.Message}");
                ErrorOccurred?.Invoke(this, ex.Message);
                return false;
            }
        }

        public async Task StopAsync()
        {
            try
            {
                if (!_isPlaying) return;

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (_audioWebView != null)
                    {
                        // Detener el audio ejecutando JavaScript
                        _audioWebView.EvaluateJavaScriptAsync("document.getElementById('radioPlayer').pause();");

                        // Remover el WebView
                        if (Application.Current?.MainPage is ContentPage currentPage)
                        {
                            if (currentPage.Content is Grid grid)
                            {
                                grid.Children.Remove(_audioWebView);
                            }
                            else if (currentPage.Content is StackLayout stack)
                            {
                                stack.Children.Remove(_audioWebView);
                            }
                        }

                        _audioWebView = null;
                    }
                });

                _isPlaying = false;
                PlayingStateChanged?.Invoke(this, false);

                System.Diagnostics.Debug.WriteLine("⏹️ Audio detenido");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error deteniendo audio: {ex.Message}");
                ErrorOccurred?.Invoke(this, ex.Message);
            }
        }

        public async Task SetVolumeAsync(double volume)
        {
            try
            {
                _currentVolume = Math.Clamp(volume, 0.0, 1.0);

                if (_audioWebView != null && _isPlaying)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await _audioWebView.EvaluateJavaScriptAsync($"document.getElementById('radioPlayer').volume = {_currentVolume};");
                    });
                }

                System.Diagnostics.Debug.WriteLine($"🔊 Volumen ajustado a: {_currentVolume * 100:F0}%");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error ajustando volumen: {ex.Message}");
            }
        }
    }
}