using PackageTracking.API.Models;

namespace PackageTracking.API.DTOs
{
    public class UpdatePackageDto
    {
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public string CurrentLocation { get; set; }
        public PackageStatus Status { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
    }
}
