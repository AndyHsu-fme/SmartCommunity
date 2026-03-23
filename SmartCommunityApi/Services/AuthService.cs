using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartCommunityApi.Data;
using SmartCommunityApi.DTOs;

namespace SmartCommunityApi.Services;

public class AuthService(SmartCommunityDbContext db, IConfiguration config) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        // 固定 Admin 帳號（先期開發用，由 appsettings.json 設定）
        var adminUnit = config["Admin:UnitNumber"] ?? "ADMIN";
        var adminPwd  = config["Admin:Password"]  ?? "Admin@2026";
        var adminName = config["Admin:UserName"]   ?? "admin";

        if (request.UnitNumber.Equals(adminUnit, StringComparison.OrdinalIgnoreCase)
            && request.Password == adminPwd)
        {
            var adminToken = GenerateJwtToken(0, adminName, adminUnit, isAdmin: true);
            return new LoginResponse(adminToken, 0, "系統管理員", adminUnit, IsAdmin: true);
        }

        // 一般住戶（BCrypt 驗證）
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.UnitNumber == request.UnitNumber);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var userToken = GenerateJwtToken(user.UserId, user.UserName, user.UnitNumber, user.IsAdmin);
        return new LoginResponse(userToken, user.UserId, user.UserName, user.UnitNumber, user.IsAdmin);
    }

    private string GenerateJwtToken(int userId, string userName, string unitNumber, bool isAdmin)
    {
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = int.TryParse(config["Jwt:ExpiryHours"], out var h) ? h : 24;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new("unitNumber", unitNumber),
            new("userName",   userName),
            new("isAdmin",    isAdmin.ToString().ToLower()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer:             config["Jwt:Issuer"],
            audience:           config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(expiry),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
