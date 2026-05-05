using PackageTracking.API.Models;

namespace PackageTracking.API.Services
{
    public interface IPackageService
    {
        Task<IEnumerable<Package>> GetAllPackagesAsync();
        Task<Package> GetPackageByIdAsync(int id);
        Task<Package> GetByTrackingNumberAsync(string trackingNumber);

        Task<Package> CreatePackageAsync(Package package);
        Task<bool> UpdatePackageAsync(int id, Package package);
        Task<bool> DeletePackageAsync(int id);
    }
}
