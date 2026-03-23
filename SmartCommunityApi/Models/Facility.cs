namespace SmartCommunityApi.Models;

public class Facility
{
    public int FacilityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }

    // Navigation properties
    public ICollection<FacilityReservation> FacilityReservations { get; set; } = [];
}
