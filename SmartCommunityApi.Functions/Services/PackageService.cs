using Microsoft.EntityFrameworkCore;
using SmartCommunityApi.Data;
using SmartCommunityApi.DTOs;
using SmartCommunityApi.Models;
using SmartCommunityApi.Models.Enums;

namespace SmartCommunityApi.Services;

public class PackageService(SmartCommunityDbContext db) : IPackageService
{
    public async Task<List<PackageDto>> GetUserPackagesAsync(int userId)
    {
        return await db.Packages
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.ArrivalDate)
            .Select(p => new PackageDto
            {
                PackageId   = p.PackageId,
                CarrierName = p.CarrierName,
                ArrivalDate = p.ArrivalDate,
                PickupDate  = p.PickupDate,
                Status      = p.Status.ToString(),
            })
            .ToListAsync();
    }

    public async Task<PackageDto> CreatePackageAsync(CreatePackageRequest request)
    {
        var pkg = new Package
        {
            UserId      = request.UserId,
            CarrierName = request.CarrierName,
            ArrivalDate = request.ArrivalDate.ToUniversalTime(),
            Status      = PackageStatus.Pending,
        };
        db.Packages.Add(pkg);
        await db.SaveChangesAsync();

        return new PackageDto
        {
            PackageId   = pkg.PackageId,
            CarrierName = pkg.CarrierName,
            ArrivalDate = pkg.ArrivalDate,
            Status      = pkg.Status.ToString(),
        };
    }

    public async Task<bool> MarkPickedUpAsync(int packageId)
    {
        var pkg = await db.Packages.FindAsync(packageId);
        if (pkg is null || pkg.Status == PackageStatus.PickedUp) return false;

        pkg.Status     = PackageStatus.PickedUp;
        pkg.PickupDate = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }
}
