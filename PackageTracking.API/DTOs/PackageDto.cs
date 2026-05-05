using PackageTracking.API.Models;

namespace PackageTracking.API.DTOs
{
    public class PackageDto
    {
        public int Id { get; set; }
        public string TrackingNumber { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public PackageStatus Status { get; set; }
        public string CurrentLocation { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
    }
}
