using System.ComponentModel.DataAnnotations;

namespace PackageTracking.API.DTOs
{
    public class CreatePackageDto
    {
        [Required]
        public string SenderName { get; set; }

        [Required]
        public string ReceiverName { get; set; }

        [Required]
        public string CurrentLocation { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }
    }
}