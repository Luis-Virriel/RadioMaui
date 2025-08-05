using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using mauiApp1Prueba.Services;

namespace mauiApp1Prueba.ViewModels
{
    public class CreateUserViewModel : INotifyPropertyChanged
    {
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly IBiometricAuthService _biometricAuthService;

        // Campos del formulario
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _fullName = string.Empty;
        private string _address = string.Empty;
        private string _phone = string.Empty;
        private string _email = string.Empty;
        private string? _profileImagePath;
        private bool _enableBiometric = false;

        // Estados de la UI
        private bool _isLoading = false;
        private bool _isBiometricAvailable = false;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private bool _showPassword = false;
        private bool _showConfirmPassword = false;

        public CreateUserViewModel(IUserService userService, IPhotoService photoService, IBiometricAuthService biometricAuthService)
        {
            _userService = userService;
            _photoService = photoService;
            _biometricAuthService = biometricAuthService;

            CreateUserCommand = new Command(async () => await ExecuteCreateUserCommand(), () => !IsLoading);
            SelectPhotoCommand = new Command(async () => await ExecuteSelectPhotoCommand(), () => !IsLoading);
            TogglePasswordVisibilityCommand = new Command(() => ShowPassword = !ShowPassword);
            ToggleConfirmPasswordVisibilityCommand = new Command(() => ShowConfirmPassword = !ShowConfirmPassword);
            ClearFormCommand = new Command(ClearForm);

            _ = CheckBiometricAvailabilityAsync();
        }

        #region Properties

        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                ClearMessages();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                ClearMessages();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                SetProperty(ref _confirmPassword, value);
                ClearMessages();
            }
        }

        public string FullName
        {
            get => _fullName;
            set
            {
                SetProperty(ref _fullName, value);
                ClearMessages();
            }
        }

        public string Address
        {
            get => _address;
            set
            {
                SetProperty(ref _address, value);
                ClearMessages();
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                SetProperty(ref _phone, value);
                ClearMessages();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
                ClearMessages();
            }
        }

        public string? ProfileImagePath
        {
            get => _profileImagePath;
            set => SetProperty(ref _profileImagePath, value);
        }

        public bool EnableBiometric
        {
            get => _enableBiometric;
            set => SetProperty(ref _enableBiometric, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                ((Command)CreateUserCommand).ChangeCanExecute();
                ((Command)SelectPhotoCommand).ChangeCanExecute();
            }
        }

        public bool IsBiometricAvailable
        {
            get => _isBiometricAvailable;
            set => SetProperty(ref _isBiometricAvailable, value);
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

        public bool ShowPassword
        {
            get => _showPassword;
            set => SetProperty(ref _showPassword, value);
        }

        public bool ShowConfirmPassword
        {
            get => _showConfirmPassword;
            set => SetProperty(ref _showConfirmPassword, value);
        }

        public bool HasProfileImage => !string.IsNullOrEmpty(ProfileImagePath);

        public string ProfileImageDisplayText => HasProfileImage ? "Cambiar foto" : "Agregar foto";

        #endregion

        #region Commands

        public ICommand CreateUserCommand { get; }
        public ICommand SelectPhotoCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand ToggleConfirmPasswordVisibilityCommand { get; }
        public ICommand ClearFormCommand { get; }

        #endregion

        #region Methods

        private async Task ExecuteCreateUserCommand()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                ClearMessages();

                // Validación básica del lado del cliente
                if (!ValidateForm())
                {
                    return;
                }

                var registrationRequest = new UserRegistrationRequest
                {
                    Username = Username.Trim(),
                    Password = Password,
                    ConfirmPassword = ConfirmPassword,
                    FullName = FullName.Trim(),
                    Address = Address.Trim(),
                    Phone = Phone.Trim(),
                    Email = Email.Trim(),
                    ProfileImagePath = ProfileImagePath,
                    EnableBiometric = EnableBiometric && IsBiometricAvailable
                };

                var (result, user) = await _userService.RegisterUserAsync(registrationRequest);

                switch (result)
                {
                    case UserRegistrationResult.Success:
                        SuccessMessage = $"¡Usuario '{user?.Username}' creado exitosamente!";
                        ClearForm();

                        // Opcional: navegar a la página principal o login
                        await Task.Delay(2000); // Mostrar mensaje por 2 segundos
                        // await Shell.Current.GoToAsync("//login");
                        break;

                    case UserRegistrationResult.UsernameAlreadyExists:
                        ErrorMessage = "El nombre de usuario ya existe. Por favor elige otro.";
                        break;

                    case UserRegistrationResult.EmailAlreadyExists:
                        ErrorMessage = "El email ya está registrado. Por favor usa otro email.";
                        break;

                    case UserRegistrationResult.InvalidEmail:
                        ErrorMessage = "El formato del email no es válido.";
                        break;

                    case UserRegistrationResult.InvalidPhone:
                        ErrorMessage = "El formato del teléfono no es válido.";
                        break;

                    case UserRegistrationResult.WeakPassword:
                        ErrorMessage = "La contraseña debe tener al menos 6 caracteres, una letra y un número.";
                        break;

                    case UserRegistrationResult.ValidationError:
                        ErrorMessage = "Por favor completa todos los campos obligatorios correctamente.";
                        break;

                    case UserRegistrationResult.DatabaseError:
                        ErrorMessage = "Error al guardar el usuario. Intenta nuevamente.";
                        break;

                    default:
                        ErrorMessage = "Error desconocido. Intenta nuevamente.";
                        break;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteSelectPhotoCommand()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;

                // Como aún no tenemos un usuario creado, usamos un ID temporal
                var tempUserId = DateTime.Now.GetHashCode();
                var photoResult = await _photoService.ShowPhotoOptionsAsync(tempUserId);

                if (photoResult.IsSuccess && !string.IsNullOrEmpty(photoResult.FilePath))
                {
                    ProfileImagePath = photoResult.FilePath;
                    OnPropertyChanged(nameof(HasProfileImage));
                    OnPropertyChanged(nameof(ProfileImageDisplayText));
                }
                else if (!string.IsNullOrEmpty(photoResult.ErrorMessage))
                {
                    ErrorMessage = photoResult.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al seleccionar foto: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "El nombre de usuario es obligatorio.";
                return false;
            }

            if (Username.Length < 3)
            {
                ErrorMessage = "El nombre de usuario debe tener al menos 3 caracteres.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "La contraseña es obligatoria.";
                return false;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Las contraseñas no coinciden.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(FullName))
            {
                ErrorMessage = "El nombre completo es obligatorio.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "El email es obligatorio.";
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            Username = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            FullName = string.Empty;
            Address = string.Empty;
            Phone = string.Empty;
            Email = string.Empty;
            ProfileImagePath = null;
            EnableBiometric = false;
            ClearMessages();

            OnPropertyChanged(nameof(HasProfileImage));
            OnPropertyChanged(nameof(ProfileImageDisplayText));
        }

        private void ClearMessages()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }

        private async Task CheckBiometricAvailabilityAsync()
        {
            try
            {
                var status = await _biometricAuthService.GetAvailabilityStatusAsync();
                IsBiometricAvailable = status == BiometricAuthStatus.Available;
            }
            catch
            {
                IsBiometricAvailable = false;
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}