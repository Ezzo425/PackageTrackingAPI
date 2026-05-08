using System.ComponentModel.DataAnnotations;
using PackageTracking.API.Models;

namespace PackageTracking.API.DTOs
{
    public class UpdatePackageDto
    {
        [Required]
        [MaxLength(100)]
        public string SenderName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ReceiverName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string CurrentLocation { get; set; } = string.Empty;

        [Required]
        [EnumDataType(typeof(PackageStatus))]
        public PackageStatus Status { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }
    }
}
