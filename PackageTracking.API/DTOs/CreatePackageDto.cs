using System.ComponentModel.DataAnnotations;

namespace PackageTracking.API.DTOs
{
    public class CreatePackageDto
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

        public DateTime? EstimatedDeliveryDate { get; set; }
    }
}