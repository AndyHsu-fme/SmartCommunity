using SmartCommunityApi.Models.Enums;

namespace SmartCommunityApi.Models;

public class Package
{
    public int PackageId { get; set; }
    public int UserId { get; set; }
    public string CarrierName { get; set; } = string.Empty;
    public DateTime ArrivalDate { get; set; }
    public DateTime? PickupDate { get; set; }
    public PackageStatus Status { get; set; } = PackageStatus.Pending;

    // Navigation properties
    public User User { get; set; } = null!;
}
