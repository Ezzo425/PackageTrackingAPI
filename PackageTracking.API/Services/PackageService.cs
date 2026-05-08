using PackageTracking.API.Models;
using PackageTracking.API.Repositories;

namespace PackageTracking.API.Services
{
    public class PackageService : IPackageService
    {
        private readonly IPackageRepository _repository;

        public PackageService(IPackageRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Package>> GetAllPackagesAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Package?> GetPackageByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Package?> GetByTrackingNumberAsync(string trackingNumber)
        {
            return await _repository.GetByTrackingNumberAsync(trackingNumber);
        }

        public async Task<Package> CreatePackageAsync(Package package)
        {
            package.CreatedAt = DateTime.UtcNow;
            package.Status = PackageStatus.Created;

            await _repository.AddAsync(package);
            await _repository.SaveChangesAsync();

            return package;
        }

        public async Task<bool> UpdatePackageAsync(int id, Package package)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return false;

            existing.SenderName = package.SenderName;
            existing.ReceiverName = package.ReceiverName;
            existing.CurrentLocation = package.CurrentLocation;
            existing.Status = package.Status;
            existing.EstimatedDeliveryDate = package.EstimatedDeliveryDate;

            _repository.Update(existing);
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeletePackageAsync(int id)
        {
            var package = await _repository.GetByIdAsync(id);
            if (package == null)
                return false;

            _repository.Delete(package);
            await _repository.SaveChangesAsync();

            return true;
        }
    }
}
