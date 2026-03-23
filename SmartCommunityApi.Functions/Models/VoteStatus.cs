namespace SmartCommunityApi.Models;

public class VoteStatus
{
    public int StatusId { get; set; }
    public int TopicId { get; set; }
    public int UserId { get; set; }
    public DateTime VotedAt { get; set; }

    // Navigation properties
    public VoteTopic VoteTopic { get; set; } = null!;
    public User User { get; set; } = null!;
}
