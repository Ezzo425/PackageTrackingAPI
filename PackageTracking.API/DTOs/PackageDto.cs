using PackageTracking.API.Models;

namespace PackageTracking.API.DTOs
{
    public class PackageDto
    {
        public int Id { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public PackageStatus Status { get; set; }
        public string CurrentLocation { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
    }
}
