using SmartCommunityApi.DTOs;

namespace SmartCommunityApi.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserAsync(int userId);
    Task<(bool Success, string? Error, UserDto? Dto)> CreateUserAsync(CreateUserRequest request);
}
