using PackageTracking.API.DTOs;
using PackageTracking.API.Models;

namespace PackageTracking.API.Mappings
{
    public class PackageMapping
    {
        public static PackageDto ToDto(Package package)
        {
            return new PackageDto
            {
                Id = package.Id, // Id of the dto is the same as the entity
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
                SenderName = dto.SenderName,
                ReceiverName = dto.ReceiverName,
                CurrentLocation = dto.CurrentLocation,
                EstimatedDeliveryDate = dto.EstimatedDeliveryDate
            };
        }
    }
}
