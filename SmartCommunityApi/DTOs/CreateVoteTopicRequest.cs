namespace SmartCommunityApi.DTOs;

public record CreateVoteTopicRequest(
    string Title,
    string? Description,
    DateTime EndTime,
    List<string> Options);
