namespace mauiApp1Prueba.Services
{
    public enum BiometricAuthStatus
    {
        Unknown,
        Available,
        NotAvailable,
        NotEnrolled,
        Denied,
        Error
    }

    public class BiometricAuthResult
    {
        public bool IsSuccess { get; set; }
        public BiometricAuthStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
    }

    public interface IBiometricAuthService
    {
        Task<BiometricAuthStatus> GetAvailabilityStatusAsync();
        Task<BiometricAuthResult> AuthenticateAsync(string reason);
        Task<bool> IsEnabledAsync();
    }

    public class BiometricAuthService : IBiometricAuthService
    {
        private readonly IPreferences _preferences;

        public BiometricAuthService(IPreferences preferences)
        {
            _preferences = preferences;
        }

        public async Task<BiometricAuthStatus> GetAvailabilityStatusAsync()
        {
            // Versión simplificada - siempre retorna no disponible
            // TODO: Implementar biometría más adelante
            return await Task.FromResult(BiometricAuthStatus.NotAvailable);
        }

        public async Task<BiometricAuthResult> AuthenticateAsync(string reason = "Autenticar con biometría")
        {
            // Versión simplificada - siempre falla
            // TODO: Implementar biometría más adelante
            return await Task.FromResult(new BiometricAuthResult
            {
                IsSuccess = false,
                Status = BiometricAuthStatus.NotAvailable,
                ErrorMessage = "Biometría no disponible en esta versión de prueba"
            });
        }

        public async Task<bool> IsEnabledAsync()
        {
            var status = await GetAvailabilityStatusAsync();
            return status == BiometricAuthStatus.Available;
        }
    }
}