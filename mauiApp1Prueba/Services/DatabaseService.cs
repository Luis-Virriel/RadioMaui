using SQLite;
using mauiApp1Prueba.Models;
using System.Security.Cryptography;
using System.Text;

namespace mauiApp1Prueba.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;
        private const string DatabaseFilename = "RadioAppDatabase.db3";

        public DatabaseService()
        {
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
            _database = new SQLiteAsyncConnection(databasePath);

            // Crear las tablas si no existen
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await _database.CreateTableAsync<User>();
        }

        #region User Methods

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _database.Table<User>().ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _database.FindAsync<User>(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _database.Table<User>()
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _database.Table<User>()
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            var existingUser = await GetUserByUsernameAsync(username);
            return existingUser == null;
        }

        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            var existingUser = await GetUserByEmailAsync(email);
            return existingUser == null;
        }

        public async Task<int> CreateUserAsync(User user)
        {
            // Generar salt y hash de la contraseña
            user.Salt = GenerateSalt();
            user.PasswordHash = HashPassword(user.PasswordHash, user.Salt);
            user.CreatedAt = DateTime.UtcNow;

            return await _database.InsertAsync(user);
        }

        public async Task<int> UpdateUserAsync(User user)
        {
            return await _database.UpdateAsync(user);
        }

        public async Task<int> DeleteUserAsync(User user)
        {
            return await _database.DeleteAsync(user);
        }

        public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null) return false;

            var hashedPassword = HashPassword(password, user.Salt);
            return hashedPassword == user.PasswordHash;
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                user.LastLogin = DateTime.UtcNow;
                await UpdateUserAsync(user);
            }
        }

        public async Task<bool> EnableBiometricAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                user.BiometricEnabled = true;
                await UpdateUserAsync(user);
                return true;
            }
            return false;
        }

        public async Task<bool> DisableBiometricAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                user.BiometricEnabled = false;
                await UpdateUserAsync(user);
                return true;
            }
            return false;
        }

        #endregion

        #region Security Methods

        private static string GenerateSalt()
        {
            using var rng = RandomNumberGenerator.Create();
            var saltBytes = new byte[32];
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        private static string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var saltedPassword = password + salt;
            var saltedPasswordBytes = Encoding.UTF8.GetBytes(saltedPassword);
            var hashBytes = sha256.ComputeHash(saltedPasswordBytes);
            return Convert.ToBase64String(hashBytes);
        }

        #endregion

        public async Task<bool> DatabaseExistsAsync()
        {
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
            return File.Exists(databasePath);
        }

        public async Task DeleteDatabaseAsync()
        {
            await _database.CloseAsync();
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }
}