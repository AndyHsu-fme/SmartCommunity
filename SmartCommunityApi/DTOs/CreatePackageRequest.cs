namespace SmartCommunityApi.DTOs;

public record CreatePackageRequest(int UserId, string CarrierName, DateTime ArrivalDate);
