using System.ComponentModel.DataAnnotations;

namespace PackageTracking.API.Models
{
    public class Package
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string TrackingNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string SenderName { get; set; }

        [Required]
        [MaxLength(100)]
        public string ReceiverName { get; set; }

        public PackageStatus Status { get; set; }

        [MaxLength(150)]
        public string CurrentLocation { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }
    }
}
