using Microsoft.AspNetCore.Mvc;
using PackageTracking.API.DTOs;
using PackageTracking.API.Mappings;
using PackageTracking.API.Services;
using Serilog;

namespace PackageTracking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PackagesController : ControllerBase
    {
        private readonly IPackageService _service;

        public PackagesController(IPackageService service)
        {
            _service = service;
        }

        // GET: api/packages
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            Log.Information("Fetching all packages");

            var packages = await _service.GetAllPackagesAsync();
            var result = packages.Select(PackageMapping.ToDto);

            return Ok(result);
        }

        // GET: api/packages/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            Log.Information("Fetching package with ID: {Id}", id);

            var package = await _service.GetPackageByIdAsync(id);

            if (package == null)
            {
                Log.Warning("Package with ID {Id} not found", id);
                return NotFound(new { message = "Package not found" });
            }

            return Ok(PackageMapping.ToDto(package));
        }

        // GET: api/packages/tracking/{trackingNumber}
        [HttpGet("tracking/{trackingNumber}")]
        public async Task<IActionResult> GetByTracking(string trackingNumber)
        {
            Log.Information("Fetching package with Tracking Number: {TrackingNumber}", trackingNumber);

            var package = await _service.GetByTrackingNumberAsync(trackingNumber);

            if (package == null)
            {
                Log.Warning("Package with Tracking Number {TrackingNumber} not found", trackingNumber);
                return NotFound(new { message = "Package not found" });
            }

            return Ok(PackageMapping.ToDto(package));
        }

        // POST: api/packages
        [HttpPost]
        public async Task<IActionResult> Create(CreatePackageDto dto)
        {
            Log.Information("Creating new package for Sender: {Sender}", dto.SenderName);

            if (!ModelState.IsValid)
            {
                Log.Warning("Invalid model state while creating package");
                return BadRequest(ModelState);
            }

            var package = PackageMapping.ToEntity(dto);

            // Generate tracking number
            package.TrackingNumber = Guid.NewGuid().ToString("N")[..8].ToUpper();

            var created = await _service.CreatePackageAsync(package);

            Log.Information("Package created successfully with ID: {Id}", created.Id);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                PackageMapping.ToDto(created)
            );
        }

        // PUT: api/packages/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdatePackageDto dto)
        {
            Log.Information("Updating package with ID: {Id}", id);

            if (!ModelState.IsValid)
            {
                Log.Warning("Invalid model state while updating package {Id}", id);
                return BadRequest(ModelState);
            }

            var package = new Models.Package
            {
                SenderName = dto.SenderName,
                ReceiverName = dto.ReceiverName,
                CurrentLocation = dto.CurrentLocation,
                Status = dto.Status,
                EstimatedDeliveryDate = dto.EstimatedDeliveryDate
            };

            var result = await _service.UpdatePackageAsync(id, package);

            if (!result)
            {
                Log.Warning("Package update failed. Package ID {Id} not found", id);
                return NotFound(new { message = "Package not found" });
            }

            Log.Information("Package updated successfully with ID: {Id}", id);

            return NoContent();
        }

        // DELETE: api/packages/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Log.Information("Deleting package with ID: {Id}", id);

            var result = await _service.DeletePackageAsync(id);

            if (!result)
            {
                Log.Warning("Delete failed. Package ID {Id} not found", id);
                return NotFound(new { message = "Package not found" });
            }

            Log.Information("Package deleted successfully with ID: {Id}", id);

            return NoContent();
        }
    }
}