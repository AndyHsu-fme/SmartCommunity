namespace SmartCommunityApi.DTOs;

public class UserDto
{
    public int UserId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}
