using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using mauiApp1Prueba.Models;
using mauiApp1Prueba.Services;
using System.Linq;

namespace mauiApp1Prueba.Views;

[QueryProperty(nameof(UserId), "userId")]
public partial class EditUserPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private User _currentUser;

    public int UserId { get; set; }

    public EditUserPage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUser();
    }

    private async Task LoadUser()
    {
        try
        {
            var users = await _databaseService.GetAllUsersAsync();
            _currentUser = users?.FirstOrDefault();

            if (_currentUser != null)
            {
                NameEntry.Text = _currentUser.FullName ?? "";
                EmailEntry.Text = _currentUser.Email ?? "";
                LoadProfileImage();
            }
            else
            {
                var create = await DisplayAlert("Sin usuarios", "No hay usuarios en la base de datos. ¿Crear uno de prueba?", "Sí", "No");
                if (create)
                {
                    await CreateTestUser();
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar usuario: {ex.Message}", "OK");
        }
    }

    private async Task CreateTestUser()
    {
        try
        {
            var testUser = new User
            {
                FullName = "Usuario de Prueba",
                Username = "testuser",
                Email = "test@ejemplo.com",
                Phone = "123456789",
                Address = "Dirección de prueba",
                PasswordHash = "test123",
                Salt = "testsalt",
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                BiometricEnabled = false
            };

            var result = await _databaseService.CreateUserAsync(testUser);
            if (result > 0)
            {
                await DisplayAlert("Éxito", "Usuario de prueba creado", "OK");
                await LoadUser();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error creando usuario: {ex.Message}", "OK");
        }
    }

    private void LoadProfileImage()
    {
        try
        {
            if (_currentUser != null && !string.IsNullOrEmpty(_currentUser.ProfileImagePath))
            {
                if (File.Exists(_currentUser.ProfileImagePath))
                {
                    // Limpiar primero y luego cargar
                    ProfileImage.Source = null;
                    ProfileImage.Source = ImageSource.FromFile(_currentUser.ProfileImagePath);
                }
                else
                {
                    ProfileImage.Source = "user_placeholder.png";
                }
            }
            else
            {
                ProfileImage.Source = "user_placeholder.png";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cargando imagen: {ex.Message}");
            ProfileImage.Source = "user_placeholder.png";
        }
    }

    private async void OnChangePhotoClicked(object sender, EventArgs e)
    {
        try
        {
            var action = await DisplayActionSheet(
                "Cambiar foto de perfil",
                "Cancelar",
                null,
                "Tomar foto",
                "Seleccionar de galería");

            FileResult result = null;

            switch (action)
            {
                case "Tomar foto":
                    if (MediaPicker.Default.IsCaptureSupported)
                    {
                        result = await MediaPicker.Default.CapturePhotoAsync();
                    }
                    else
                    {
                        await DisplayAlert("No disponible", "La cámara no está disponible", "OK");
                        return;
                    }
                    break;
                case "Seleccionar de galería":
                    result = await MediaPicker.Default.PickPhotoAsync();
                    break;
            }

            if (result != null)
            {
                await SavePhotoDirectly(result);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
    }

    private async Task SavePhotoDirectly(FileResult photo)
    {
        try
        {
            if (_currentUser == null)
            {
                await DisplayAlert("Error", "No hay usuario cargado", "OK");
                return;
            }

            // Crear nombre único
            var fileName = $"profile_{_currentUser.Id}_{DateTime.Now:yyyyMMddHHmmss}.jpg";
            var localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            System.Diagnostics.Debug.WriteLine($"Guardando imagen en: {localPath}");

            // Guardar archivo
            using (var sourceStream = await photo.OpenReadAsync())
            using (var fileStream = File.Create(localPath))
            {
                await sourceStream.CopyToAsync(fileStream);
            }

            // Verificar que se guardó
            if (File.Exists(localPath))
            {
                System.Diagnostics.Debug.WriteLine($"Archivo guardado exitosamente. Tamaño: {new FileInfo(localPath).Length} bytes");

                // Actualizar usuario
                _currentUser.ProfileImagePath = localPath;
                var updateResult = await _databaseService.UpdateUserAsync(_currentUser);

                System.Diagnostics.Debug.WriteLine($"Resultado update BD: {updateResult}");

                if (updateResult > 0)
                {
                    // Actualizar imagen en UI
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ProfileImage.Source = ImageSource.FromFile(localPath);
                    });

                    await DisplayAlert("Éxito", $"Foto guardada: {Path.GetFileName(localPath)}", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo actualizar en la base de datos", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "No se pudo guardar el archivo", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en SavePhotoDirectly: {ex}");
            await DisplayAlert("Error", $"Error al guardar: {ex.Message}", "OK");
        }
    }

    private async void OnRemovePhotoClicked(object sender, EventArgs e)
    {
        try
        {
            if (_currentUser != null)
            {
                if (!string.IsNullOrEmpty(_currentUser.ProfileImagePath) && File.Exists(_currentUser.ProfileImagePath))
                {
                    File.Delete(_currentUser.ProfileImagePath);
                }

                _currentUser.ProfileImagePath = null;
                await _databaseService.UpdateUserAsync(_currentUser);

                ProfileImage.Source = "user_placeholder.png";
                await DisplayAlert("Éxito", "Foto eliminada", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            if (_currentUser != null)
            {
                _currentUser.FullName = NameEntry.Text?.Trim();
                _currentUser.Email = EmailEntry.Text?.Trim();

                var result = await _databaseService.UpdateUserAsync(_currentUser);
                if (result > 0)
                {
                    await DisplayAlert("Éxito", "Perfil actualizado", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo actualizar el perfil", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}