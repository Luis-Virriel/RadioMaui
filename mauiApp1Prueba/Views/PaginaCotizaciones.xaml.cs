using mauiApp1Prueba.Models;
using mauiApp1Prueba.Services;

namespace mauiApp1Prueba.Views;

public partial class PaginaCotizaciones : ContentPage
{
    private readonly CurrencyService _currencyService;
    private bool _isLoading = false;

    public PaginaCotizaciones()
    {
        InitializeComponent();
        _currencyService = new CurrencyService();
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object sender, EventArgs e)
    {
        await LoadRatesAsync();
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadRatesAsync();
    }

    private async Task LoadRatesAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            // Mostrar indicadores de carga
            SetLoadingState(true);

            var rates = await _currencyService.GetRatesAsync();

            // Actualizar las etiquetas
            UpdateCurrencyLabels(rates);

            // Mostrar éxito
            SetSuccessState();
        }
        catch (Exception ex)
        {
            // Manejar error
            SetErrorState(ex);
        }
        finally
        {
            _isLoading = false;
            var refreshButton = this.FindByName<Button>("RefreshButton");
            if (refreshButton != null)
                refreshButton.IsEnabled = true;
        }
    }

    private void SetLoadingState(bool isLoading)
    {
        var loadingIndicator = this.FindByName<ActivityIndicator>("LoadingIndicator");
        var statusLabel = this.FindByName<Label>("StatusLabel");
        var refreshButton = this.FindByName<Button>("RefreshButton");

        if (loadingIndicator != null)
        {
            loadingIndicator.IsVisible = isLoading;
            loadingIndicator.IsRunning = isLoading;
        }

        if (statusLabel != null)
        {
            statusLabel.IsVisible = isLoading;
            if (isLoading)
            {
                statusLabel.Text = "Obteniendo cotizaciones...";
                statusLabel.TextColor = Colors.Blue;
            }
        }

        if (refreshButton != null)
            refreshButton.IsEnabled = !isLoading;
    }

    private void UpdateCurrencyLabels(dynamic rates)
    {
        var usdLabel = this.FindByName<Label>("UsdLabel");
        var eurLabel = this.FindByName<Label>("EurLabel");
        var brlLabel = this.FindByName<Label>("BrlLabel");
        var lastUpdatedLabel = this.FindByName<Label>("LastUpdatedLabel");

        if (usdLabel != null)
            usdLabel.Text = $"$ {rates.UsdUyu:N2}";

        if (eurLabel != null)
            eurLabel.Text = $"$ {rates.EurUyu:N2}";

        if (brlLabel != null)
            brlLabel.Text = $"$ {rates.BrlUyu:N2}";

        if (lastUpdatedLabel != null)
            lastUpdatedLabel.Text = $"Última actualización: {rates.LastUpdatedLocal:dd/MM/yyyy HH:mm}";
    }

    private async void SetSuccessState()
    {
        var loadingIndicator = this.FindByName<ActivityIndicator>("LoadingIndicator");
        var statusLabel = this.FindByName<Label>("StatusLabel");

        if (loadingIndicator != null)
        {
            loadingIndicator.IsVisible = false;
            loadingIndicator.IsRunning = false;
        }

        if (statusLabel != null)
        {
            statusLabel.Text = "✅ Cotizaciones actualizadas";
            statusLabel.TextColor = Colors.Green;
            statusLabel.IsVisible = true;

            // Ocultar después de 2 segundos
            await Task.Delay(2000);
            if (statusLabel.Text.Contains("✅"))
                statusLabel.IsVisible = false;
        }
    }

    private async void SetErrorState(Exception ex)
    {
        var loadingIndicator = this.FindByName<ActivityIndicator>("LoadingIndicator");
        var statusLabel = this.FindByName<Label>("StatusLabel");
        var usdLabel = this.FindByName<Label>("UsdLabel");
        var eurLabel = this.FindByName<Label>("EurLabel");
        var brlLabel = this.FindByName<Label>("BrlLabel");
        var lastUpdatedLabel = this.FindByName<Label>("LastUpdatedLabel");

        if (loadingIndicator != null)
        {
            loadingIndicator.IsVisible = false;
            loadingIndicator.IsRunning = false;
        }

        string errorMessage = "Error al cargar cotizaciones";
        string statusText = "❌ Error al cargar";

        if (ex.Message.Contains("401") || ex.Message.Contains("101"))
        {
            errorMessage = "🔑 Error de autenticación: Verifica tu API Key";
            statusText = "❌ Error de autenticación";
        }
        else if (ex.Message.Contains("104"))
        {
            errorMessage = "📊 Límite de solicitudes mensuales excedido";
            statusText = "❌ Límite excedido";
        }
        else if (ex.Message.Contains("conexión") || ex is HttpRequestException)
        {
            errorMessage = "🌐 Error de conexión a internet";
            statusText = "❌ Sin conexión";
        }
        else if (ex is TaskCanceledException)
        {
            errorMessage = "⏱️ Tiempo de espera agotado";
            statusText = "❌ Tiempo agotado";
        }

        if (statusLabel != null)
        {
            statusLabel.Text = statusText;
            statusLabel.TextColor = Colors.Red;
            statusLabel.IsVisible = true;
        }

        await DisplayAlert("Error", errorMessage, "OK");

        // Mostrar valores por defecto
        if (usdLabel != null) usdLabel.Text = "$ --,--";
        if (eurLabel != null) eurLabel.Text = "$ --,--";
        if (brlLabel != null) brlLabel.Text = "$ --,--";
        if (lastUpdatedLabel != null) lastUpdatedLabel.Text = "Última actualización: Error al obtener datos";
    }
}