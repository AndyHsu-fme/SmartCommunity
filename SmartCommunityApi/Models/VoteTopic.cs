namespace SmartCommunityApi.Models;

public class VoteTopic
{
    public int TopicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EndTime { get; set; }
    /// <summary>JSON 序列化的選項陣列，例如 ["贊成","反對","棄權"]</summary>
    public string? OptionsJson { get; set; }

    // Navigation properties
    public ICollection<VoteStatus> VoteStatuses { get; set; } = [];
    public ICollection<AnonymousBallot> AnonymousBallots { get; set; } = [];
}
