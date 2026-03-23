namespace SmartCommunityApi.DTOs;

public class PackageDto
{
    public int PackageId { get; set; }
    public string CarrierName { get; set; } = string.Empty;
    public DateTime ArrivalDate { get; set; }
    public DateTime? PickupDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
