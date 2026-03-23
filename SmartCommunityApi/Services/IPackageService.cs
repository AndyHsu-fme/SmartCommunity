using SmartCommunityApi.DTOs;

namespace SmartCommunityApi.Services;

public interface IPackageService
{
    Task<List<PackageDto>> GetUserPackagesAsync(int userId);
    Task<PackageDto> CreatePackageAsync(CreatePackageRequest request);
    Task<bool> MarkPickedUpAsync(int packageId);
}
