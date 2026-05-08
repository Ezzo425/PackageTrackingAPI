using PackageTracking.API.Models;

namespace PackageTracking.API.Repositories
{
    public interface IPackageRepository
    {
        Task<IEnumerable<Package>> GetAllAsync();
        Task<Package?> GetByIdAsync(int id);
        Task<Package?> GetByTrackingNumberAsync(string trackingNumber);

        Task AddAsync(Package package);
        void Update(Package package);
        void Delete(Package package);

        Task SaveChangesAsync();
    }
}
