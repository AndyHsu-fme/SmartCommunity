namespace SmartCommunityApi.Models;

/// <summary>
/// 匿名投票記錄 — 刻意不含 UserId，以確保投票匿名性。
/// </summary>
public class AnonymousBallot
{
    public int BallotId { get; set; }
    public int TopicId { get; set; }
    public string OptionSelected { get; set; } = string.Empty;

    /// <summary>
    /// 單向雜湊令牌，用於防止重複投票偵測，但無法反查 UserId。
    /// </summary>
    public string HashToken { get; set; } = string.Empty;

    // Navigation properties
    public VoteTopic VoteTopic { get; set; } = null!;
}
