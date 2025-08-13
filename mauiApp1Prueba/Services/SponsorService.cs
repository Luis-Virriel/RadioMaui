using SQLite;
using mauiApp1Prueba.Models;

namespace mauiApp1Prueba.Services
{
    public class SponsorService : ISponsorService
    {
        private readonly SQLiteAsyncConnection _database;

        public SponsorService()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "sponsors.db3");
            _database = new SQLiteAsyncConnection(dbPath);
            InitializeDatabaseAsync();
        }

        private async void InitializeDatabaseAsync()
        {
            await _database.CreateTableAsync<Sponsor>();
        }

        public async Task<IEnumerable<Sponsor>> GetAllSponsorsAsync()
        {
            try
            {
                return await _database.Table<Sponsor>()
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error getting sponsors: {ex.Message}");
                return new List<Sponsor>();
            }
        }

        public async Task<Sponsor?> GetSponsorByIdAsync(int id)
        {
            try
            {
                return await _database.Table<Sponsor>()
                    .Where(s => s.Id == id && s.IsActive)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting sponsor by ID: {ex.Message}");
                return null;
            }
        }

        public async Task<int> AddSponsorAsync(Sponsor sponsor)
        {
            try
            {
                if (sponsor == null)
                    throw new ArgumentNullException(nameof(sponsor));

                sponsor.CreatedAt = DateTime.UtcNow;
                return await _database.InsertAsync(sponsor);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding sponsor: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateSponsorAsync(Sponsor sponsor)
        {
            try
            {
                if (sponsor == null)
                    throw new ArgumentNullException(nameof(sponsor));

                var result = await _database.UpdateAsync(sponsor);
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating sponsor: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteSponsorAsync(int id)
        {
            try
            {
                var sponsor = await GetSponsorByIdAsync(id);
                if (sponsor != null)
                {
                    // Soft delete
                    sponsor.IsActive = false;
                    return await UpdateSponsorAsync(sponsor);
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting sponsor: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<Sponsor>> GetSponsorsByLocationAsync(double latitude, double longitude, double radiusKm = 10)
        {
            try
            {
                var allSponsors = await GetAllSponsorsAsync();
                var userLocation = new SponsorLocation(latitude, longitude);

                return allSponsors.Where(sponsor =>
                {
                    var sponsorLocation = new SponsorLocation(sponsor.Latitude, sponsor.Longitude);
                    var distance = userLocation.DistanceTo(sponsorLocation);
                    return distance <= radiusKm;
                }).OrderBy(sponsor =>
                {
                    var sponsorLocation = new SponsorLocation(sponsor.Latitude, sponsor.Longitude);
                    return userLocation.DistanceTo(sponsorLocation);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting sponsors by location: {ex.Message}");
                return new List<Sponsor>();
            }
        }

        public async Task<bool> SponsorExistsAsync(int id)
        {
            try
            {
                var sponsor = await GetSponsorByIdAsync(id);
                return sponsor != null;
            }
            catch
            {
                return false;
            }
        }
    }
}