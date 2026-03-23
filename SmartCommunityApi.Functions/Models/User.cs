namespace SmartCommunityApi.Models;

public class User
{
    public int UserId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }

    // Navigation properties
    public ICollection<VoteStatus> VoteStatuses { get; set; } = [];
    public ICollection<Package> Packages { get; set; } = [];
    public ICollection<FacilityReservation> FacilityReservations { get; set; } = [];
}
