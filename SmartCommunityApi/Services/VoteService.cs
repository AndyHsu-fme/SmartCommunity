using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCommunityApi.Data;
using SmartCommunityApi.DTOs;
using SmartCommunityApi.Models;

namespace SmartCommunityApi.Services;

public class VoteService(SmartCommunityDbContext db, IConfiguration config) : IVoteService
{
    public async Task<List<VoteTopicDto>> GetActiveTopicsAsync(int userId)
    {
        var topics = await db.VoteTopics
            .Where(t => t.EndTime > DateTime.UtcNow)
            .ToListAsync();

        var votedTopicIds = await db.VoteStatuses
            .Where(vs => vs.UserId == userId)
            .Select(vs => vs.TopicId)
            .ToHashSetAsync();

        return topics.Select(t => new VoteTopicDto
        {
            TopicId     = t.TopicId,
            Title       = t.Title,
            Description = t.Description,
            EndTime     = t.EndTime,
            HasVoted    = votedTopicIds.Contains(t.TopicId),
            Options     = ParseOptions(t.OptionsJson),
        }).ToList();
    }

    public async Task<CastVoteResult> CastVoteAsync(int userId, int topicId, string option)
    {
        var topic = await db.VoteTopics.FindAsync(topicId);
        if (topic is null) return CastVoteResult.TopicNotFound;
        if (topic.EndTime < DateTime.UtcNow) return CastVoteResult.TopicExpired;

        bool alreadyVoted = await db.VoteStatuses
            .AnyAsync(vs => vs.TopicId == topicId && vs.UserId == userId);
        if (alreadyVoted) return CastVoteResult.AlreadyVoted;

        // 紀錄「誰投過票」（含 UserId）
        db.VoteStatuses.Add(new VoteStatus
        {
            TopicId = topicId,
            UserId  = userId,
            VotedAt = DateTime.UtcNow,
        });

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            // 唯一索引衝突（並發重複投票）
            return CastVoteResult.AlreadyVoted;
        }

        // 紀錄「投了什麼票」（不含 UserId，確保匿名性）
        db.AnonymousBallots.Add(new AnonymousBallot
        {
            TopicId        = topicId,
            OptionSelected = option,
            HashToken      = GenerateHashToken(userId, topicId),
        });
        await db.SaveChangesAsync();

        return CastVoteResult.Success;
    }

    public async Task<VoteTopicWithResultsDto> CreateTopicAsync(CreateVoteTopicRequest request)
    {
        var topic = new VoteTopic
        {
            Title       = request.Title,
            Description = request.Description,
            EndTime     = request.EndTime.ToUniversalTime(),
            OptionsJson = JsonSerializer.Serialize(request.Options),
        };
        db.VoteTopics.Add(topic);
        await db.SaveChangesAsync();

        return MapToResultsDto(topic, []);
    }

    public async Task<List<VoteTopicWithResultsDto>> GetAllTopicsWithResultsAsync()
    {
        var topics  = await db.VoteTopics.ToListAsync();
        var ballots = await db.AnonymousBallots.ToListAsync();

        return topics.Select(t =>
        {
            var topicBallots = ballots.Where(b => b.TopicId == t.TopicId).ToList();
            return MapToResultsDto(t, topicBallots);
        }).ToList();
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static VoteTopicWithResultsDto MapToResultsDto(VoteTopic topic, List<AnonymousBallot> ballots)
    {
        var options = ParseOptions(topic.OptionsJson);
        var total   = ballots.Count;

        var results = options.Select(opt =>
        {
            var count = ballots.Count(b => b.OptionSelected == opt);
            return new VoteOptionResult
            {
                Option     = opt,
                Count      = count,
                Percentage = total > 0 ? Math.Round((double)count / total * 100, 1) : 0,
            };
        }).ToList();

        // 統計未在預設選項中的票（防止資料遺漏）
        var knownOptions = options.ToHashSet(StringComparer.Ordinal);
        foreach (var g in ballots
            .Where(b => !knownOptions.Contains(b.OptionSelected))
            .GroupBy(b => b.OptionSelected))
        {
            results.Add(new VoteOptionResult
            {
                Option     = g.Key,
                Count      = g.Count(),
                Percentage = total > 0 ? Math.Round((double)g.Count() / total * 100, 1) : 0,
            });
        }

        return new VoteTopicWithResultsDto
        {
            TopicId     = topic.TopicId,
            Title       = topic.Title,
            Description = topic.Description,
            EndTime     = topic.EndTime,
            IsExpired   = topic.EndTime < DateTime.UtcNow,
            TotalVotes  = total,
            Options     = options,
            Results     = results,
        };
    }

    private static List<string> ParseOptions(string? json)
    {
        if (string.IsNullOrEmpty(json)) return [];
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? []; }
        catch { return []; }
    }

    private string GenerateHashToken(int userId, int topicId)
    {
        var secret  = config["HashToken:Secret"] ?? "default-hash-secret";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var message = $"{userId}:{topicId}";
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(message)));
    }
}
