using mauiApp1Prueba.Models;

namespace mauiApp1Prueba.Services
{
    public interface ISponsorService
    {
        Task<IEnumerable<Sponsor>> GetAllSponsorsAsync();
        Task<Sponsor?> GetSponsorByIdAsync(int id);
        Task<int> AddSponsorAsync(Sponsor sponsor);
        Task<bool> UpdateSponsorAsync(Sponsor sponsor);
        Task<bool> DeleteSponsorAsync(int id);
        Task<IEnumerable<Sponsor>> GetSponsorsByLocationAsync(double latitude, double longitude, double radiusKm = 10);
        Task<bool> SponsorExistsAsync(int id);
    }
}