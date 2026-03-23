using SmartCommunityApi.Models;
using SmartCommunityApi.Models.Enums;
using SmartCommunityApi.Tests.Helpers;
using Xunit;

namespace SmartCommunityApi.Tests;

public class FacilityReservationTests
{
    [Fact]
    public async Task AddFacility_ShouldPersist()
    {
        // Arrange
        await using var ctx = DbContextFactory.Create();

        // Act
        var facility = new Facility { Name = "B1 健身房", MaxCapacity = 10 };
        ctx.Facilities.Add(facility);
        await ctx.SaveChangesAsync();

        // Assert
        var saved = await ctx.Facilities.FindAsync(facility.FacilityId);
        Assert.NotNull(saved);
        Assert.Equal("B1 健身房", saved.Name);
        Assert.Equal(10, saved.MaxCapacity);
    }

    [Fact]
    public async Task AddReservation_DefaultStatusShouldBePending()
    {
        // Arrange
        await using var ctx = DbContextFactory.Create();
        var user = new User { UnitNumber = "F501", UserName = "住戶甲", PasswordHash = "h1" };
        var facility = new Facility { Name = "頂樓游泳池", MaxCapacity = 20 };
        ctx.Users.Add(user);
        ctx.Facilities.Add(facility);
        await ctx.SaveChangesAsync();

        // Act
        var reservation = new FacilityReservation
        {
            FacilityId = facility.FacilityId,
            UserId = user.UserId,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        };
        ctx.FacilityReservations.Add(reservation);
        await ctx.SaveChangesAsync();

        // Assert
        var saved = await ctx.FacilityReservations.FindAsync(reservation.ReservationId);
        Assert.NotNull(saved);
        Assert.Equal(ReservationStatus.Pending, saved.Status);
    }

    [Fact]
    public async Task CancelReservation_ShouldUpdateStatus()
    {
        // Arrange
        await using var ctx = DbContextFactory.Create();
        var user = new User { UnitNumber = "F502", UserName = "住戶乙", PasswordHash = "h2" };
        var facility = new Facility { Name = "社區交誼廳", MaxCapacity = 30 };
        ctx.Users.Add(user);
        ctx.Facilities.Add(facility);
        await ctx.SaveChangesAsync();

        var reservation = new FacilityReservation
        {
            FacilityId = facility.FacilityId,
            UserId = user.UserId,
            StartTime = DateTime.UtcNow.AddHours(3),
            EndTime = DateTime.UtcNow.AddHours(4),
            Status = ReservationStatus.Confirmed
        };
        ctx.FacilityReservations.Add(reservation);
        await ctx.SaveChangesAsync();

        // Act
        reservation.Status = ReservationStatus.Cancelled;
        await ctx.SaveChangesAsync();

        // Assert
        var saved = await ctx.FacilityReservations.FindAsync(reservation.ReservationId);
        Assert.NotNull(saved);
        Assert.Equal(ReservationStatus.Cancelled, saved.Status);
    }

    [Fact]
    public async Task CheckTimeOverlap_ShouldDetectConflict()
    {
        // Arrange
        await using var ctx = DbContextFactory.Create();
        var facility = new Facility { Name = "羽球場", MaxCapacity = 4 };
        ctx.Facilities.Add(facility);
        await ctx.SaveChangesAsync();

        var startA = new DateTime(2026, 4, 1, 9, 0, 0, DateTimeKind.Utc);
        var endA = new DateTime(2026, 4, 1, 11, 0, 0, DateTimeKind.Utc);

        ctx.FacilityReservations.Add(new FacilityReservation
        {
            FacilityId = facility.FacilityId,
            UserId = 1,
            StartTime = startA,
            EndTime = endA,
            Status = ReservationStatus.Confirmed
        });
        await ctx.SaveChangesAsync();

        // Act — 查詢是否有重疊時段
        var requestStart = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var requestEnd = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc);

        var conflict = ctx.FacilityReservations.Any(r =>
            r.FacilityId == facility.FacilityId &&
            r.Status != ReservationStatus.Cancelled &&
            r.StartTime < requestEnd &&
            r.EndTime > requestStart);

        // Assert
        Assert.True(conflict);
    }
}
