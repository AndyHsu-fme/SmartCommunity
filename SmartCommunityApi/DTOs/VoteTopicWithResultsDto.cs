namespace SmartCommunityApi.DTOs;

public class VoteOptionResult
{
    public string Option { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class VoteTopicWithResultsDto
{
    public int TopicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsExpired { get; set; }
    public int TotalVotes { get; set; }
    public List<string> Options { get; set; } = [];
    public List<VoteOptionResult> Results { get; set; } = [];
}
