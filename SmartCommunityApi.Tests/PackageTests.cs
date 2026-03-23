using SmartCommunityApi.Models;
using SmartCommunityApi.Models.Enums;
using SmartCommunityApi.Tests.Helpers;
using Xunit;

namespace SmartCommunityApi.Tests;

public class PackageTests
{
    [Fact]
    public async Task AddPackage_DefaultStatusShouldBePending()
    {
        // Arrange
        await using var ctx = DbContextFactory.Create();
        var user = new User { UnitNumber = "D401", UserName = "居民甲", PasswordHash = "h1" };
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        // Act
        var pkg = new Package
        {
            UserId = user.UserId,
            CarrierName = "黑貓宅急便",
            ArrivalDate = DateTime.UtcNow
        };
        ctx.Packages.Add(pkg);
        await ctx.SaveChangesAsync();

        // Assert
        var saved = await ctx.Packages.FindAsync(pkg.PackageId);
        Assert.NotNull(saved);
        Assert.Equal(PackageStatus.Pending, saved.Status);
        Assert.Null(saved.PickupDate);
    }

    [Fact]
    public async Task MarkPackageAsPickedUp_ShouldUpdateStatus()
    {
        // Arrange
        await using var ctx = DbContextFactory.Create();
        var user = new User { UnitNumber = "D402", UserName = "居民乙", PasswordHash = "h2" };
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var pkg = new Package
        {
            UserId = user.UserId,
            CarrierName = "統一速達",
            ArrivalDate = DateTime.UtcNow.AddDays(-1)
        };
        ctx.Packages.Add(pkg);
        await ctx.SaveChangesAsync();

        // Act
        pkg.Status = PackageStatus.PickedUp;
        pkg.PickupDate = DateTime.UtcNow;
        await ctx.SaveChangesAsync();

        // Assert
        var saved = await ctx.Packages.FindAsync(pkg.PackageId);
        Assert.NotNull(saved);
        Assert.Equal(PackageStatus.PickedUp, saved.Status);
        Assert.NotNull(saved.PickupDate);
    }

    [Fact]
    public async Task UserPackages_ShouldReturnOnlyOwnPackages()
    {
        // Arrange
        await using var ctx = DbContextFactory.Create();
        var userA = new User { UnitNumber = "E101", UserName = "住戶A", PasswordHash = "hA" };
        var userB = new User { UnitNumber = "E102", UserName = "住戶B", PasswordHash = "hB" };
        ctx.Users.AddRange(userA, userB);
        await ctx.SaveChangesAsync();

        ctx.Packages.AddRange(
            new Package { UserId = userA.UserId, CarrierName = "7-11取貨", ArrivalDate = DateTime.UtcNow },
            new Package { UserId = userA.UserId, CarrierName = "郵局", ArrivalDate = DateTime.UtcNow },
            new Package { UserId = userB.UserId, CarrierName = "蝦皮", ArrivalDate = DateTime.UtcNow }
        );
        await ctx.SaveChangesAsync();

        // Act
        var userAPackages = ctx.Packages.Where(p => p.UserId == userA.UserId).ToList();

        // Assert
        Assert.Equal(2, userAPackages.Count);
        Assert.All(userAPackages, p => Assert.Equal(userA.UserId, p.UserId));
    }
}
