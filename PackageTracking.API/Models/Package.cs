using System.ComponentModel.DataAnnotations;

namespace PackageTracking.API.Models
{
    public class Package
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string TrackingNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string SenderName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ReceiverName { get; set; } = string.Empty;

        public PackageStatus Status { get; set; }

        [Required]
        [MaxLength(150)]
        public string CurrentLocation { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }
    }
}
