using SmartCommunityApi.Models.Enums;

namespace SmartCommunityApi.Models;

public class FacilityReservation
{
    public int ReservationId { get; set; }
    public int FacilityId { get; set; }
    public int UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    // Navigation properties
    public Facility Facility { get; set; } = null!;
    public User User { get; set; } = null!;
}
