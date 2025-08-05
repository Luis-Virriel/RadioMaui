using mauiApp1Prueba.Models;
using System.Text.RegularExpressions;

namespace mauiApp1Prueba.Services
{
    public enum UserRegistrationResult
    {
        Success,
        UsernameAlreadyExists,
        EmailAlreadyExists,
        InvalidEmail,
        InvalidPhone,
        WeakPassword,
        ValidationError,
        DatabaseError
    }

    public enum UserLoginResult
    {
        Success,
        InvalidCredentials,
        UserNotFound,
        BiometricAuthRequired,
        BiometricAuthFailed,
        DatabaseError
    }

    public class UserRegistrationRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfileImagePath { get; set; }
        public bool EnableBiometric { get; set; } = false;
    }

    public class UserLoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool UseBiometric { get; set; } = false;
    }

    public interface IUserService
    {
        Task<(UserRegistrationResult Result, User? User)> RegisterUserAsync(UserRegistrationRequest request);
        Task<(UserLoginResult Result, User? User)> LoginAsync(UserLoginRequest request);
        Task<(UserLoginResult Result, User? User)> BiometricLoginAsync(string username);
        Task<bool> IsUsernameAvailableAsync(string username);
        Task<bool> IsEmailAvailableAsync(string email);
        Task<User?> GetCurrentUserAsync();
        Task LogoutAsync();
        Task<bool> UpdateUserProfileAsync(User user);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> EnableBiometricAsync(int userId);
        Task<bool> DisableBiometricAsync(int userId);
    }

    public class UserService : IUserService
    {
        private readonly DatabaseService _databaseService;
        private readonly IBiometricAuthService _biometricAuthService;
        private readonly IPreferences _preferences;
        private User? _currentUser;

        private const string CurrentUserIdKey = "current_user_id";
        private const string BiometricEnabledKey = "biometric_enabled";
        private const string LastUsernameKey = "last_username";

        public UserService(DatabaseService databaseService, IBiometricAuthService biometricAuthService, IPreferences preferences)
        {
            _databaseService = databaseService;
            _biometricAuthService = biometricAuthService;
            _preferences = preferences;
        }

        public async Task<(UserRegistrationResult Result, User? User)> RegisterUserAsync(UserRegistrationRequest request)
        {
            try
            {
                // Validaciones básicas
                if (!IsValidRegistrationRequest(request, out var validationResult))
                {
                    return (validationResult, null);
                }

                // Verificar disponibilidad de username
                if (!await _databaseService.IsUsernameAvailableAsync(request.Username))
                {
                    return (UserRegistrationResult.UsernameAlreadyExists, null);
                }

                // Verificar disponibilidad de email
                if (!await _databaseService.IsEmailAvailableAsync(request.Email))
                {
                    return (UserRegistrationResult.EmailAlreadyExists, null);
                }

                // Crear nuevo usuario
                var user = new User
                {
                    Username = request.Username.Trim().ToLower(),
                    PasswordHash = request.Password, // Se hashea en DatabaseService
                    FullName = request.FullName.Trim(),
                    Address = request.Address.Trim(),
                    Phone = request.Phone.Trim(),
                    Email = request.Email.Trim().ToLower(),
                    ProfileImagePath = request.ProfileImagePath,
                    BiometricEnabled = request.EnableBiometric,
                    CreatedAt = DateTime.UtcNow
                };

                await _databaseService.CreateUserAsync(user);

                // Si habilitó biométrica, configurarla
                if (request.EnableBiometric)
                {
                    var biometricStatus = await _biometricAuthService.GetAvailabilityStatusAsync();
                    if (biometricStatus == BiometricAuthStatus.Available)
                    {
                        _preferences.Set($"{BiometricEnabledKey}_{user.Username}", true);
                    }
                    else
                    {
                        user.BiometricEnabled = false;
                        await _databaseService.UpdateUserAsync(user);
                    }
                }

                return (UserRegistrationResult.Success, user);
            }
            catch (Exception)
            {
                return (UserRegistrationResult.DatabaseError, null);
            }
        }

        public async Task<(UserLoginResult Result, User? User)> LoginAsync(UserLoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return (UserLoginResult.InvalidCredentials, null);
                }

                var user = await _databaseService.GetUserByUsernameAsync(request.Username.Trim().ToLower());
                if (user == null)
                {
                    return (UserLoginResult.UserNotFound, null);
                }

                var isValidPassword = await _databaseService.ValidateUserCredentialsAsync(request.Username.Trim().ToLower(), request.Password);
                if (!isValidPassword)
                {
                    return (UserLoginResult.InvalidCredentials, null);
                }

                await SetCurrentUserAsync(user);
                await _databaseService.UpdateLastLoginAsync(user.Id);

                _preferences.Set(LastUsernameKey, user.Username);

                return (UserLoginResult.Success, user);
            }
            catch (Exception)
            {
                return (UserLoginResult.DatabaseError, null);
            }
        }

        public async Task<(UserLoginResult Result, User? User)> BiometricLoginAsync(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    var lastUsername = _preferences.Get(LastUsernameKey, string.Empty);
                    if (string.IsNullOrWhiteSpace(lastUsername))
                    {
                        return (UserLoginResult.UserNotFound, null);
                    }
                    username = lastUsername;
                }

                var user = await _databaseService.GetUserByUsernameAsync(username.Trim().ToLower());
                if (user == null)
                {
                    return (UserLoginResult.UserNotFound, null);
                }

                if (!user.BiometricEnabled)
                {
                    return (UserLoginResult.BiometricAuthRequired, null);
                }

                var biometricResult = await _biometricAuthService.AuthenticateAsync("Accede a Radio App con tu biometría");
                if (!biometricResult.IsSuccess)
                {
                    return (UserLoginResult.BiometricAuthFailed, null);
                }

                await SetCurrentUserAsync(user);
                await _databaseService.UpdateLastLoginAsync(user.Id);

                return (UserLoginResult.Success, user);
            }
            catch (Exception)
            {
                return (UserLoginResult.DatabaseError, null);
            }
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            return await _databaseService.IsUsernameAvailableAsync(username.Trim().ToLower());
        }

        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return await _databaseService.IsEmailAvailableAsync(email.Trim().ToLower());
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            if (_currentUser != null) return _currentUser;

            var userId = _preferences.Get(CurrentUserIdKey, -1);
            if (userId == -1) return null;

            _currentUser = await _databaseService.GetUserByIdAsync(userId);
            return _currentUser;
        }

        public async Task LogoutAsync()
        {
            _currentUser = null;
            _preferences.Remove(CurrentUserIdKey);
        }

        public async Task<bool> UpdateUserProfileAsync(User user)
        {
            try
            {
                await _databaseService.UpdateUserAsync(user);
                if (_currentUser?.Id == user.Id)
                {
                    _currentUser = user;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _databaseService.GetUserByIdAsync(userId);
                if (user == null) return false;

                var isValidCurrentPassword = await _databaseService.ValidateUserCredentialsAsync(user.Username, currentPassword);
                if (!isValidCurrentPassword) return false;

                if (!IsValidPassword(newPassword)) return false;

                // Actualizar contraseña
                user.PasswordHash = newPassword; // Se rehashea en DatabaseService
                user.Salt = string.Empty; // Se regenera en DatabaseService
                await _databaseService.UpdateUserAsync(user);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EnableBiometricAsync(int userId)
        {
            try
            {
                var biometricStatus = await _biometricAuthService.GetAvailabilityStatusAsync();
                if (biometricStatus != BiometricAuthStatus.Available) return false;

                var result = await _databaseService.EnableBiometricAsync(userId);
                if (result && _currentUser?.Id == userId)
                {
                    _currentUser.BiometricEnabled = true;
                    _preferences.Set($"{BiometricEnabledKey}_{_currentUser.Username}", true);
                }
                return result;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DisableBiometricAsync(int userId)
        {
            try
            {
                var result = await _databaseService.DisableBiometricAsync(userId);
                if (result && _currentUser?.Id == userId)
                {
                    _currentUser.BiometricEnabled = false;
                    _preferences.Remove($"{BiometricEnabledKey}_{_currentUser.Username}");
                }
                return result;
            }
            catch
            {
                return false;
            }
        }

        #region Private Methods

        private async Task SetCurrentUserAsync(User user)
        {
            _currentUser = user;
            _preferences.Set(CurrentUserIdKey, user.Id);
        }

        private static bool IsValidRegistrationRequest(UserRegistrationRequest request, out UserRegistrationResult result)
        {
            result = UserRegistrationResult.ValidationError;

            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.ConfirmPassword) ||
                string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Email))
            {
                return false;
            }

            if (request.Password != request.ConfirmPassword)
            {
                return false;
            }

            if (!IsValidPassword(request.Password))
            {
                result = UserRegistrationResult.WeakPassword;
                return false;
            }

            if (!IsValidEmail(request.Email))
            {
                result = UserRegistrationResult.InvalidEmail;
                return false;
            }

            if (!IsValidPhone(request.Phone))
            {
                result = UserRegistrationResult.InvalidPhone;
                return false;
            }

            if (request.Username.Length < 3 || request.Username.Length > 50)
            {
                return false;
            }

            result = UserRegistrationResult.Success;
            return true;
        }

        private static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            if (password.Length < 6) return false;

            // Al menos una letra y un número
            var hasLetter = password.Any(char.IsLetter);
            var hasDigit = password.Any(char.IsDigit);

            return hasLetter && hasDigit;
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            try
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                return emailRegex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;

            // Permitir números con espacios, guiones y paréntesis
            var phoneRegex = new Regex(@"^[\d\s\-\(\)\+]{8,20}$");
            return phoneRegex.IsMatch(phone);
        }

        #endregion
    }
}