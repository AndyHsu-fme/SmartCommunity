using Microsoft.EntityFrameworkCore;
using SmartCommunityApi.Data;
using SmartCommunityApi.DTOs;
using SmartCommunityApi.Models;

namespace SmartCommunityApi.Services;

public class UserService(SmartCommunityDbContext db) : IUserService
{
    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await db.Users
            .OrderBy(u => u.UnitNumber)
            .Select(u => new UserDto
            {
                UserId     = u.UserId,
                UnitNumber = u.UnitNumber,
                UserName   = u.UserName,
                IsAdmin    = u.IsAdmin,
            })
            .ToListAsync();
    }

    public async Task<UserDto?> GetUserAsync(int userId)
    {
        var u = await db.Users.FindAsync(userId);
        if (u is null) return null;
        return new UserDto
        {
            UserId     = u.UserId,
            UnitNumber = u.UnitNumber,
            UserName   = u.UserName,
            IsAdmin    = u.IsAdmin,
        };
    }

    public async Task<(bool Success, string? Error, UserDto? Dto)> CreateUserAsync(CreateUserRequest request)
    {
        bool exists = await db.Users.AnyAsync(u => u.UnitNumber == request.UnitNumber);
        if (exists)
            return (false, "門牌號碼已存在", null);

        var user = new User
        {
            UnitNumber   = request.UnitNumber,
            UserName     = request.UserName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsAdmin      = request.IsAdmin,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (true, null, new UserDto
        {
            UserId     = user.UserId,
            UnitNumber = user.UnitNumber,
            UserName   = user.UserName,
            IsAdmin    = user.IsAdmin,
        });
    }
}
