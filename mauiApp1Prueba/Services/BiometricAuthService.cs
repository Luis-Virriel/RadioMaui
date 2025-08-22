#if ANDROID
using Android.App;
using AndroidX.Biometric;
using AndroidX.Core.Content;
using AndroidX.Fragment.App;
using System.Threading.Tasks;
#endif
using Microsoft.Maui.Storage;

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
#if ANDROID
            var activity = Platform.CurrentActivity;
            if (activity == null)
                return BiometricAuthStatus.NotAvailable;

            var canAuthenticate = BiometricManager.From(activity)
                .CanAuthenticate(BiometricManager.Authenticators.BiometricStrong);

            return canAuthenticate switch
            {
                BiometricManager.BiometricSuccess => BiometricAuthStatus.Available,
                BiometricManager.BiometricErrorNoHardware => BiometricAuthStatus.NotAvailable,
                BiometricManager.BiometricErrorHwUnavailable => BiometricAuthStatus.NotAvailable,
                BiometricManager.BiometricErrorNoneEnrolled => BiometricAuthStatus.NotEnrolled,
                _ => BiometricAuthStatus.Error
            };
#else
            return await Task.FromResult(BiometricAuthStatus.NotAvailable);
#endif
        }

        public async Task<BiometricAuthResult> AuthenticateAsync(string reason = "Autenticar con biometría")
        {
#if ANDROID
            var status = await GetAvailabilityStatusAsync();
            if (status != BiometricAuthStatus.Available)
            {
                return new BiometricAuthResult
                {
                    IsSuccess = false,
                    Status = status,
                    ErrorMessage = "Biometría no disponible o no configurada"
                };
            }

            var tcs = new TaskCompletionSource<BiometricAuthResult>();
            var activity = Platform.CurrentActivity;

            if (activity == null)
            {
                return new BiometricAuthResult
                {
                    IsSuccess = false,
                    Status = BiometricAuthStatus.Error,
                    ErrorMessage = "No se pudo obtener la actividad actual"
                };
            }

            var fragmentActivity = activity as FragmentActivity;
            if (fragmentActivity == null)
            {
                return new BiometricAuthResult
                {
                    IsSuccess = false,
                    Status = BiometricAuthStatus.Error,
                    ErrorMessage = "No se pudo convertir a FragmentActivity"
                };
            }

            var executor = ContextCompat.GetMainExecutor(fragmentActivity);
            var callback = new BiometricPromptAuthenticationCallback(tcs);
            var biometricPrompt = new BiometricPrompt(fragmentActivity, executor, callback);

            var promptInfo = new BiometricPrompt.PromptInfo.Builder()
                .SetTitle("Autenticación")
                .SetSubtitle(reason)
                .SetNegativeButtonText("Cancelar")
                .Build();

            biometricPrompt.Authenticate(promptInfo);

            return await tcs.Task;
#else
            return await Task.FromResult(new BiometricAuthResult
            {
                IsSuccess = false,
                Status = BiometricAuthStatus.NotAvailable,
                ErrorMessage = "Biometría no soportada en esta plataforma"
            });
#endif
        }

        public async Task<bool> IsEnabledAsync()
        {
            var status = await GetAvailabilityStatusAsync();
            return status == BiometricAuthStatus.Available;
        }

#if ANDROID
        private class BiometricPromptAuthenticationCallback : BiometricPrompt.AuthenticationCallback
        {
            private readonly TaskCompletionSource<BiometricAuthResult> _tcs;

            public BiometricPromptAuthenticationCallback(TaskCompletionSource<BiometricAuthResult> tcs)
            {
                _tcs = tcs;
            }

            public override void OnAuthenticationSucceeded(BiometricPrompt.AuthenticationResult result)
            {
                base.OnAuthenticationSucceeded(result);
                _tcs.TrySetResult(new BiometricAuthResult
                {
                    IsSuccess = true,
                    Status = BiometricAuthStatus.Available
                });
            }

            public override void OnAuthenticationFailed()
            {
                base.OnAuthenticationFailed();
                _tcs.TrySetResult(new BiometricAuthResult
                {
                    IsSuccess = false,
                    Status = BiometricAuthStatus.Error,
                    ErrorMessage = "Autenticación fallida"
                });
            }

            public override void OnAuthenticationError(int errorCode, Java.Lang.ICharSequence errString)
            {
                base.OnAuthenticationError(errorCode, errString);
                _tcs.TrySetResult(new BiometricAuthResult
                {
                    IsSuccess = false,
                    Status = BiometricAuthStatus.Error,
                    ErrorMessage = errString?.ToString()
                });
            }
        }
#endif
    }
}
