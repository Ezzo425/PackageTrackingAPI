using PackageTracking.API.DTOs;
using PackageTracking.API.Models;

namespace PackageTracking.API.Mappings
{
    public static class PackageMapping
    {
        public static PackageDto ToDto(Package package)
        {
            return new PackageDto
            {
                Id = package.Id,
                TrackingNumber = package.TrackingNumber,
                SenderName = package.SenderName,
                ReceiverName = package.ReceiverName,
                Status = package.Status,
                CurrentLocation = package.CurrentLocation,
                CreatedAt = package.CreatedAt,
                EstimatedDeliveryDate = package.EstimatedDeliveryDate
            };
        }

        public static Package ToEntity(CreatePackageDto dto)
        {
            return new Package
            {
                TrackingNumber = string.Empty,
                SenderName = dto.SenderName,
                ReceiverName = dto.ReceiverName,
                CurrentLocation = dto.CurrentLocation,
                EstimatedDeliveryDate = dto.EstimatedDeliveryDate
            };
        }
    }
}
