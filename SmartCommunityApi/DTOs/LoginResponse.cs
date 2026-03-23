namespace SmartCommunityApi.DTOs;

public record LoginResponse(
    string Token,
    int UserId,
    string UserName,
    string UnitNumber,
    bool IsAdmin);
