using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using mauiApp1Prueba.Models;
using mauiApp1Prueba.Services;

namespace mauiApp1Prueba.ViewModels;

public class EditUserViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _databaseService;

    // Usuario actual
    private User _user;
    private User _originalUser;

    // Campos adicionales no incluidos en el modelo base
    private string _userBio = string.Empty;

    // Estados de la UI
    private bool _isLoading = false;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _hasUnsavedChanges = false;

    public EditUserViewModel()
    {
        _databaseService = new DatabaseService();
        _user = new User();
        _originalUser = new User();

        // Inicializar comandos
        SaveCommand = new Command(async () => await ExecuteSaveCommand(), () => !IsLoading && HasUnsavedChanges);
        CancelCommand = new Command(async () => await ExecuteCancelCommand());
        ChangePhotoCommand = new Command(async () => await ExecuteChangePhotoCommand(), () => !IsLoading);
        RemovePhotoCommand = new Command(async () => await ExecuteRemovePhotoCommand(), () => !IsLoading && HasProfileImage);
        ResetFormCommand = new Command(ResetForm);
        ToggleBiometricCommand = new Command(async () => await ExecuteToggleBiometricCommand());
    }

    public EditUserViewModel(User user) : this()
    {
        if (user != null)
        {
            SetUser(user);
        }
    }

    #region Properties

    public User User
    {
        get => _user;
        set
        {
            if (_user != value)
            {
                _user = value ?? new User();
                OnPropertyChanged();
                RefreshUserProperties();
            }
        }
    }

    public string UserBio
    {
        get => _userBio;
        set
        {
            if (SetProperty(ref _userBio, value))
            {
                CheckForChanges();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                RefreshCommandStates();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string SuccessMessage
    {
        get => _successMessage;
        set => SetProperty(ref _successMessage, value);
    }

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        private set
        {
            if (SetProperty(ref _hasUnsavedChanges, value))
            {
                RefreshCommandStates();
            }
        }
    }

    // Propiedades calculadas
    public bool IsUserLoaded => User != null && User.Id > 0;

    public string DisplayName => !string.IsNullOrEmpty(User?.FullName)
        ? User.FullName
        : User?.Username ?? "Usuario";

    public string ProfileImageSource => !string.IsNullOrEmpty(User?.ProfileImagePath)
        ? User.ProfileImagePath
        : "user_placeholder.png";

    public string FormattedCreatedAt => User?.CreatedAt.ToString("dd/MM/yyyy HH:mm") ?? "";

    public string FormattedLastLogin => User?.LastLogin != default(DateTime)
        ? User.LastLogin.ToString("dd/MM/yyyy HH:mm")
        : "Nunca";

    public bool HasBiometricEnabled => User?.BiometricEnabled ?? false;

    public bool HasProfileImage => !string.IsNullOrEmpty(User?.ProfileImagePath);

    public string BiometricButtonText => HasBiometricEnabled ? "Deshabilitar biométrico" : "Habilitar biométrico";

    public string ChangePhotoButtonText => HasProfileImage ? "Cambiar foto" : "Agregar foto";

    #endregion

    #region Commands

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ChangePhotoCommand { get; }
    public ICommand RemovePhotoCommand { get; }
    public ICommand ResetFormCommand { get; }
    public ICommand ToggleBiometricCommand { get; }

    #endregion

    #region Command Implementations

    private async Task ExecuteSaveCommand()
    {
        if (IsLoading || !HasUnsavedChanges) return;

        try
        {
            IsLoading = true;
            ClearMessages();

            // Validaciones
            if (!await ValidateUniqueFields())
            {
                return;
            }

            // Actualizar en la base de datos
            var result = await _databaseService.UpdateUserAsync(User);

            if (result > 0)
            {
                SuccessMessage = "Los cambios se guardaron correctamente";

                // Actualizar el usuario original
                _originalUser = CloneUser(User);
                HasUnsavedChanges = false;

                // Limpiar mensaje después de unos segundos
                _ = Task.Delay(3000).ContinueWith(_ =>
                {
                    MainThread.BeginInvokeOnMainThread(() => SuccessMessage = string.Empty);
                });
            }
            else
            {
                ErrorMessage = "No se pudieron guardar los cambios. Inténtalo de nuevo.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al guardar: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteCancelCommand()
    {
        if (HasUnsavedChanges)
        {
            // En una implementación real, mostrarías un diálogo de confirmación
            // Por ahora, simplemente restauramos los valores originales
            ResetForm();
        }
    }

    private async Task ExecuteChangePhotoCommand()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;

            // Aquí implementarías la selección de foto
            // Por ahora, simulo la funcionalidad
            await Task.Delay(1000); // Simular operación async

            // Ejemplo de implementación básica:
            // var result = await MediaPicker.PickPhotoAsync();
            // if (result != null)
            // {
            //     var localFilePath = await SavePhotoLocally(result);
            //     User.ProfileImagePath = localFilePath;
            //     CheckForChanges();
            //     RefreshUserProperties();
            // }

            SuccessMessage = "Funcionalidad de cambio de foto pendiente de implementar";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cambiar foto: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteRemovePhotoCommand()
    {
        if (IsLoading || !HasProfileImage) return;

        try
        {
            IsLoading = true;

            // Eliminar archivo físico si existe
            if (File.Exists(User.ProfileImagePath))
            {
                File.Delete(User.ProfileImagePath);
            }

            User.ProfileImagePath = null;
            CheckForChanges();
            RefreshUserProperties();

            SuccessMessage = "Foto de perfil eliminada";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al eliminar foto: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteToggleBiometricCommand()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            ClearMessages();

            bool newBiometricState = !User.BiometricEnabled;

            if (newBiometricState)
            {
                var result = await _databaseService.EnableBiometricAsync(User.Id);
                if (result)
                {
                    User.BiometricEnabled = true;
                    SuccessMessage = "Autenticación biométrica habilitada";
                }
                else
                {
                    ErrorMessage = "No se pudo habilitar la autenticación biométrica";
                }
            }
            else
            {
                var result = await _databaseService.DisableBiometricAsync(User.Id);
                if (result)
                {
                    User.BiometricEnabled = false;
                    SuccessMessage = "Autenticación biométrica deshabilitada";
                }
                else
                {
                    ErrorMessage = "No se pudo deshabilitar la autenticación biométrica";
                }
            }

            CheckForChanges();
            OnPropertyChanged(nameof(HasBiometricEnabled));
            OnPropertyChanged(nameof(BiometricButtonText));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cambiar configuración biométrica: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Helper Methods

    public void SetUser(User user)
    {
        User = user;
        _originalUser = CloneUser(user);
        HasUnsavedChanges = false;
        ClearMessages();
    }

    public void UpdateUserProperty(string propertyName, object value)
    {
        var property = typeof(User).GetProperty(propertyName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(User, value);
            CheckForChanges();
            OnPropertyChanged(nameof(User));
        }
    }

    private void ResetForm()
    {
        if (_originalUser != null)
        {
            User = CloneUser(_originalUser);
            HasUnsavedChanges = false;
            ClearMessages();
            RefreshUserProperties();
        }
    }

    private void CheckForChanges()
    {
        if (_originalUser == null) return;

        HasUnsavedChanges =
            _originalUser.FullName != User.FullName ||
            _originalUser.Username != User.Username ||
            _originalUser.Email != User.Email ||
            _originalUser.Phone != User.Phone ||
            _originalUser.Address != User.Address ||
            _originalUser.ProfileImagePath != User.ProfileImagePath ||
            _originalUser.BiometricEnabled != User.BiometricEnabled;
    }

    private async Task<bool> ValidateUniqueFields()
    {
        // Validar username único (excluyendo el usuario actual)
        if (_originalUser.Username != User.Username)
        {
            var isUsernameAvailable = await _databaseService.IsUsernameAvailableAsync(User.Username);
            if (!isUsernameAvailable)
            {
                ErrorMessage = "El nombre de usuario ya está en uso. Por favor elige otro.";
                return false;
            }
        }

        // Validar email único (excluyendo el usuario actual)
        if (_originalUser.Email != User.Email)
        {
            var isEmailAvailable = await _databaseService.IsEmailAvailableAsync(User.Email);
            if (!isEmailAvailable)
            {
                ErrorMessage = "El correo electrónico ya está en uso. Por favor usa otro.";
                return false;
            }
        }

        return true;
    }

    private User CloneUser(User user)
    {
        return new User
        {
            Id = user.Id,
            Username = user.Username,
            PasswordHash = user.PasswordHash,
            Salt = user.Salt,
            FullName = user.FullName,
            Address = user.Address,
            Phone = user.Phone,
            Email = user.Email,
            ProfileImagePath = user.ProfileImagePath,
            BiometricEnabled = user.BiometricEnabled,
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastLogin
        };
    }

    private void ClearMessages()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }

    private void RefreshCommandStates()
    {
        ((Command)SaveCommand).ChangeCanExecute();
        ((Command)ChangePhotoCommand).ChangeCanExecute();
        ((Command)RemovePhotoCommand).ChangeCanExecute();
    }

    public void RefreshUserProperties()
    {
        OnPropertyChanged(nameof(IsUserLoaded));
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(ProfileImageSource));
        OnPropertyChanged(nameof(FormattedCreatedAt));
        OnPropertyChanged(nameof(FormattedLastLogin));
        OnPropertyChanged(nameof(HasBiometricEnabled));
        OnPropertyChanged(nameof(HasProfileImage));
        OnPropertyChanged(nameof(BiometricButtonText));
        OnPropertyChanged(nameof(ChangePhotoButtonText));
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}