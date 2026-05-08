using Microsoft.EntityFrameworkCore;
using PackageTracking.API.Data;
using PackageTracking.API.Models;

namespace PackageTracking.API.Repositories
{
    public class PackageRepository : IPackageRepository
    {
        private readonly AppDbContext _context;

        public PackageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Package>> GetAllAsync()
        {
            return await _context.Packages.ToListAsync();
        }

        public async Task<Package?> GetByIdAsync(int id)
        {
            return await _context.Packages.FindAsync(id);
        }

        public async Task<Package?> GetByTrackingNumberAsync(string trackingNumber)
        {
            return await _context.Packages
                .FirstOrDefaultAsync(p => p.TrackingNumber == trackingNumber);
        }

        public async Task AddAsync(Package package)
        {
            await _context.Packages.AddAsync(package);
        }

        public void Update(Package package)
        {
            _context.Packages.Update(package);
        }

        public void Delete(Package package)
        {
            _context.Packages.Remove(package);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
