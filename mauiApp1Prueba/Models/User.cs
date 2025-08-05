using SQLite;
namespace mauiApp1Prueba.Models
{
    [Table("Users")]
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique, NotNull]
        public string Username { get; set; } = string.Empty;

        [NotNull]
        public string PasswordHash { get; set; } = string.Empty;

        [NotNull]
        public string FullName { get; set; } = string.Empty;

        [NotNull]
        public string Address { get; set; } = string.Empty;

        [NotNull]
        public string Phone { get; set; } = string.Empty;

        [NotNull]
        public string Email { get; set; } = string.Empty;

        // Ruta local del archivo de imagen en el dispositivo
        public string? ProfileImagePath { get; set; }

        public bool BiometricEnabled { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastLogin { get; set; }

        // Salt para el hash de la contraseña
        public string Salt { get; set; } = string.Empty;
    }
}