using Microsoft.EntityFrameworkCore;
using SmartCommunityApi.Data;
using SmartCommunityApi.DTOs;
using SmartCommunityApi.Models;
using SmartCommunityApi.Models.Enums;

namespace SmartCommunityApi.Services;

public class ReservationService(SmartCommunityDbContext db) : IReservationService
{
    public async Task<bool> CheckAvailabilityAsync(int facilityId, DateTime start, DateTime end)
    {
        return !await db.FacilityReservations
            .AnyAsync(r =>
                r.FacilityId == facilityId &&
                r.Status != ReservationStatus.Cancelled &&
                r.StartTime < end &&
                r.EndTime   > start);
    }

    public async Task<List<FacilityDto>> GetFacilitiesAsync()
    {
        return await db.Facilities
            .Select(f => new FacilityDto
            {
                FacilityId   = f.FacilityId,
                Name         = f.Name,
                MaxCapacity  = f.MaxCapacity,
            })
            .ToListAsync();
    }

    public async Task<List<ReservationDto>> GetUserReservationsAsync(int userId)
    {
        return await db.FacilityReservations
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.StartTime)
            .Select(r => new ReservationDto
            {
                ReservationId = r.ReservationId,
                FacilityId    = r.FacilityId,
                FacilityName  = r.Facility.Name,
                StartTime     = r.StartTime,
                EndTime       = r.EndTime,
                Status        = r.Status.ToString(),
            })
            .ToListAsync();
    }

    public async Task<(CreateReservationResult Result, ReservationDto? Dto)> CreateReservationAsync(
        int userId, CreateReservationRequest request)
    {
        var facility = await db.Facilities.FindAsync(request.FacilityId);
        if (facility is null)
            return (CreateReservationResult.FacilityNotFound, null);

        var start = request.StartTime.ToUniversalTime();
        var end   = request.EndTime.ToUniversalTime();

        bool available = await CheckAvailabilityAsync(request.FacilityId, start, end);
        if (!available)
            return (CreateReservationResult.Conflict, null);

        var reservation = new FacilityReservation
        {
            FacilityId = request.FacilityId,
            UserId     = userId,
            StartTime  = start,
            EndTime    = end,
            Status     = ReservationStatus.Confirmed,
        };
        db.FacilityReservations.Add(reservation);
        await db.SaveChangesAsync();

        return (CreateReservationResult.Success, new ReservationDto
        {
            ReservationId = reservation.ReservationId,
            FacilityId    = reservation.FacilityId,
            FacilityName  = facility.Name,
            StartTime     = reservation.StartTime,
            EndTime       = reservation.EndTime,
            Status        = reservation.Status.ToString(),
        });
    }

    public async Task<bool> CancelReservationAsync(int reservationId, int userId)
    {
        var reservation = await db.FacilityReservations
            .FirstOrDefaultAsync(r => r.ReservationId == reservationId && r.UserId == userId);

        if (reservation is null || reservation.Status == ReservationStatus.Cancelled)
            return false;

        reservation.Status = ReservationStatus.Cancelled;
        await db.SaveChangesAsync();
        return true;
    }
}
