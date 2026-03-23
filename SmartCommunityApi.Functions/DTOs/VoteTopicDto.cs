namespace SmartCommunityApi.DTOs;

public class VoteTopicDto
{
    public int TopicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EndTime { get; set; }
    public bool HasVoted { get; set; }
    public List<string> Options { get; set; } = [];
}
