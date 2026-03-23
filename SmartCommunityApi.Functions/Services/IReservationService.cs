using SmartCommunityApi.DTOs;

namespace SmartCommunityApi.Services;

public enum CreateReservationResult { Success, Conflict, FacilityNotFound }

public interface IReservationService
{
    Task<bool> CheckAvailabilityAsync(int facilityId, DateTime start, DateTime end);
    Task<List<FacilityDto>> GetFacilitiesAsync();
    Task<List<ReservationDto>> GetUserReservationsAsync(int userId);
    Task<(CreateReservationResult Result, ReservationDto? Dto)> CreateReservationAsync(int userId, CreateReservationRequest request);
    Task<bool> CancelReservationAsync(int reservationId, int userId);
}
