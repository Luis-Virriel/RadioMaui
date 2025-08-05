namespace mauiApp1Prueba.Services
{
    public enum PhotoSource
    {
        Camera,
        Gallery
    }

    public class PhotoResult
    {
        public bool IsSuccess { get; set; }
        public string? FilePath { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public interface IPhotoService
    {
        Task<PhotoResult> TakePhotoAsync(int userId);
        Task<PhotoResult> PickPhotoAsync(int userId);
        Task<bool> DeletePhotoAsync(string filePath);
        Task<string> GetProfileImagePathAsync(int userId);
        Task<bool> PhotoExistsAsync(string filePath);
    }

    public class PhotoService : IPhotoService
    {
        private const string ProfilePhotosFolder = "ProfilePhotos";

        public async Task<PhotoResult> TakePhotoAsync(int userId)
        {
            try
            {
                // Verificar si la cámara está disponible
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    return new PhotoResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "La cámara no está disponible en este dispositivo"
                    };
                }

                // Tomar la foto
                var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = "Tomar foto de perfil"
                });

                if (photo == null)
                {
                    return new PhotoResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "No se tomó ninguna foto"
                    };
                }

                // Guardar la foto en el directorio de la app
                var savedPath = await SavePhotoAsync(photo, userId);

                return new PhotoResult
                {
                    IsSuccess = true,
                    FilePath = savedPath
                };
            }
            catch (Exception ex)
            {
                return new PhotoResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error al tomar la foto: {ex.Message}"
                };
            }
        }

        public async Task<PhotoResult> PickPhotoAsync(int userId)
        {
            try
            {
                var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Seleccionar foto de perfil"
                });

                if (photo == null)
                {
                    return new PhotoResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "No se seleccionó ninguna foto"
                    };
                }

                // Guardar la foto en el directorio de la app
                var savedPath = await SavePhotoAsync(photo, userId);

                return new PhotoResult
                {
                    IsSuccess = true,
                    FilePath = savedPath
                };
            }
            catch (Exception ex)
            {
                return new PhotoResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error al seleccionar la foto: {ex.Message}"
                };
            }
        }

        public async Task<bool> DeletePhotoAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                    return false;

                File.Delete(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetProfileImagePathAsync(int userId)
        {
            var fileName = $"user_{userId}_profile.jpg";
            var profilePhotosPath = Path.Combine(FileSystem.AppDataDirectory, ProfilePhotosFolder);
            return Path.Combine(profilePhotosPath, fileName);
        }

        public async Task<bool> PhotoExistsAsync(string filePath)
        {
            return !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
        }

        #region Private Methods

        private async Task<string> SavePhotoAsync(FileResult photo, int userId)
        {
            // Crear directorio si no existe
            var profilePhotosPath = Path.Combine(FileSystem.AppDataDirectory, ProfilePhotosFolder);
            Directory.CreateDirectory(profilePhotosPath);

            // Generar nombre único para la foto
            var fileName = $"user_{userId}_profile_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            var filePath = Path.Combine(profilePhotosPath, fileName);

            // Eliminar foto anterior si existe
            await DeleteOldProfilePhotoAsync(userId);

            // Copiar la nueva foto
            using var sourceStream = await photo.OpenReadAsync();
            using var destinationStream = File.Create(filePath);
            await sourceStream.CopyToAsync(destinationStream);

            return filePath;
        }

        private async Task DeleteOldProfilePhotoAsync(int userId)
        {
            try
            {
                var profilePhotosPath = Path.Combine(FileSystem.AppDataDirectory, ProfilePhotosFolder);
                if (!Directory.Exists(profilePhotosPath))
                    return;

                // Buscar y eliminar fotos anteriores del usuario
                var existingFiles = Directory.GetFiles(profilePhotosPath, $"user_{userId}_profile_*.jpg");
                foreach (var file in existingFiles)
                {
                    File.Delete(file);
                }
            }
            catch
            {
                // Ignorar errores al eliminar fotos anteriores
            }
        }

        #endregion
    }

    // Extensión para facilitar el uso en ViewModels
    public static class PhotoServiceExtensions
    {
        public static async Task<PhotoResult> ShowPhotoOptionsAsync(this IPhotoService photoService, int userId)
        {
            try
            {
                var action = await Shell.Current.DisplayActionSheet(
                    "Seleccionar foto de perfil",
                    "Cancelar",
                    null,
                    "Tomar foto",
                    "Seleccionar de galería");

                return action switch
                {
                    "Tomar foto" => await photoService.TakePhotoAsync(userId),
                    "Seleccionar de galería" => await photoService.PickPhotoAsync(userId),
                    _ => new PhotoResult { IsSuccess = false, ErrorMessage = "Operación cancelada" }
                };
            }
            catch (Exception ex)
            {
                return new PhotoResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error: {ex.Message}"
                };
            }
        }
    }
}