namespace SmartCommunityApi.DTOs;

public record CreateReservationRequest(int FacilityId, DateTime StartTime, DateTime EndTime);
