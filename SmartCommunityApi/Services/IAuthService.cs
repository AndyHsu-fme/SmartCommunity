using SmartCommunityApi.DTOs;

namespace SmartCommunityApi.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}
