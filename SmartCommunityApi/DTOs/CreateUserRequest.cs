namespace SmartCommunityApi.DTOs;

public record CreateUserRequest(string UnitNumber, string UserName, string Password, bool IsAdmin = false);
