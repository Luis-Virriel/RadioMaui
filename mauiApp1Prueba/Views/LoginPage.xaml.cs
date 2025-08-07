using mauiApp1Prueba.Models;
using mauiApp1Prueba.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace mauiApp1Prueba.Views;

public partial class LoginPage : ContentPage
{
    private readonly IUserService _userService;
    private readonly IBiometricAuthService _biometricAuthService;
    private bool _isPasswordVisible = false;

    public LoginPage(IUserService userService, IBiometricAuthService biometricAuthService)
    {
        InitializeComponent();
        _userService = userService;
        _biometricAuthService = biometricAuthService;

        // Verificar disponibilidad de biometría al cargar (sin bloquear UI)
        _ = CheckBiometricAvailabilityAsync();
    }

    #region Login Methods

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            await SetLoadingState(true);
            await HideErrorMessage();

            if (!await ValidateLoginFieldsAsync())
                return;

            var loginRequest = new UserLoginRequest
            {
                Username = UsernameEntry.Text.Trim(),
                Password = PasswordEntry.Text
            };

            var (result, user) = await _userService.LoginAsync(loginRequest);

            await HandleLoginResult(result, user);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"Error inesperado: {ex.Message}");
        }
        finally
        {
            await SetLoadingState(false);
        }
    }

    private async void OnBiometricLoginClicked(object sender, EventArgs e)
    {
        try
        {
            await SetLoadingState(true);
            await HideErrorMessage();

            var (result, user) = await _userService.BiometricLoginAsync(string.Empty);
            await HandleLoginResult(result, user);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"Error en biometría: {ex.Message}");
        }
        finally
        {
            await SetLoadingState(false);
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        try
        {
            var createUserPage = Handler?.MauiContext?.Services?.GetService<CreateUserPage>();
            if (createUserPage != null)
            {
                await Navigation.PushAsync(createUserPage);
            }
            else
            {
                await ShowErrorMessage("Error al cargar la página de registro");
            }
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"Error: {ex.Message}");
        }
    }

    private void OnTogglePasswordVisibility(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;
        ShowPasswordButton.Text = _isPasswordVisible ? "🙈" : "👁️";
    }

    #endregion

    #region Helper Methods

    private async Task<bool> ValidateLoginFieldsAsync()
    {
        if (string.IsNullOrWhiteSpace(UsernameEntry?.Text))
        {
            await ShowErrorMessage("Por favor ingresa tu nombre de usuario");
            UsernameEntry?.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(PasswordEntry?.Text))
        {
            await ShowErrorMessage("Por favor ingresa tu contraseña");
            PasswordEntry?.Focus();
            return false;
        }

        if (PasswordEntry.Text.Length < 6)
        {
            await ShowErrorMessage("La contraseña debe tener al menos 6 caracteres");
            PasswordEntry?.Focus();
            return false;
        }

        return true;
    }

    private async Task HandleLoginResult(UserLoginResult result, Models.User? user)
    {
        switch (result)
        {
            case UserLoginResult.Success:
                await ShowSuccessAndNavigate(user);
                break;

            case UserLoginResult.InvalidCredentials:
                await ShowErrorMessage("Usuario o contraseña incorrectos");
                break;

            case UserLoginResult.UserNotFound:
                await ShowErrorMessage("Usuario no encontrado");
                break;

            case UserLoginResult.BiometricAuthFailed:
                await ShowErrorMessage("Falló la autenticación biométrica");
                break;

            case UserLoginResult.BiometricAuthRequired:
                await ShowErrorMessage("La biometría no está configurada para este usuario");
                break;

            case UserLoginResult.DatabaseError:
                await ShowErrorMessage("Error de conexión. Intenta nuevamente.");
                break;

            default:
                await ShowErrorMessage("Error desconocido. Intenta nuevamente.");
                break;
        }
    }

    private async Task ShowSuccessAndNavigate(User? user)
{
    var welcomeMessage = !string.IsNullOrEmpty(user?.FullName)
        ? $"¡Bienvenido, {user.FullName}!"
        : "¡Bienvenido!";

    await DisplayAlert("Éxito", welcomeMessage, "Continuar");
    if (Application.Current is App app)
    {
        app.IniciarShell();
    }
}




    private Task SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        LoginButton.IsEnabled = !isLoading;
        BiometricButton.IsEnabled = !isLoading;
        RegisterButton.IsEnabled = !isLoading;

        LoginButton.Text = isLoading ? "Iniciando sesión..." : "Iniciar Sesión";

        return Task.CompletedTask;
    }

    private async Task ShowErrorMessage(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;

        var timer = Application.Current?.Dispatcher.CreateTimer();
        if (timer != null)
        {
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += (s, e) =>
            {
                ErrorLabel.IsVisible = false;
                timer.Stop();
            };
            timer.Start();
        }
    }

    private Task HideErrorMessage()
    {
        ErrorLabel.IsVisible = false;
        return Task.CompletedTask;
    }

    private async Task CheckBiometricAvailabilityAsync()
    {
        try
        {
            var status = await _biometricAuthService.GetAvailabilityStatusAsync();
            BiometricButton.IsVisible = status == BiometricAuthStatus.Available;
        }
        catch
        {
            BiometricButton.IsVisible = false;
        }
    }

    #endregion

    #region Page Events

    protected override void OnAppearing()
    {
        base.OnAppearing();

        UsernameEntry.Text = string.Empty;
        PasswordEntry.Text = string.Empty;
        ErrorLabel.IsVisible = false;
    }

    #endregion
}
