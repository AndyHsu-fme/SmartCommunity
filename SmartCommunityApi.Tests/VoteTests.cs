using SmartCommunityApi.Models;
using SmartCommunityApi.Tests.Helpers;
using Xunit;

namespace SmartCommunityApi.Tests;

public class VoteTests
{
    [Fact]
    public async Task AddVoteTopic_ShouldPersist()
    {
        // Arrange
        await using var ctx = DbContextFactory.Create();
        var topic = new VoteTopic
        {
            Title = "2026 年度管委會選舉",
            Description = "請投票選出新任管委會成員",
            EndTime = DateTime.UtcNow.AddDays(7)
        };

        // Act
        ctx.VoteTopics.Add(topic);
        await ctx.SaveChangesAsync();

        // Assert
        var saved = await ctx.VoteTopics.FindAsync(topic.TopicId);
        Assert.NotNull(saved);
        Assert.Equal("2026 年度管委會選舉", saved.Title);
    }

    [Fact]
    public async Task VoteStatus_ShouldLinkUserAndTopic()
    {
        // Arrange
        await using var ctx = DbContextFactory.Create();

        var user = new User { UnitNumber = "B201", UserName = "居民甲", PasswordHash = "h1" };
        var topic = new VoteTopic { Title = "社區綠化投票", EndTime = DateTime.UtcNow.AddDays(3) };
        ctx.Users.Add(user);
        ctx.VoteTopics.Add(topic);
        await ctx.SaveChangesAsync();

        // Act
        var status = new VoteStatus
        {
            TopicId = topic.TopicId,
            UserId = user.UserId,
            VotedAt = DateTime.UtcNow
        };
        ctx.VoteStatuses.Add(status);
        await ctx.SaveChangesAsync();

        // Assert
        var saved = await ctx.VoteStatuses.FindAsync(status.StatusId);
        Assert.NotNull(saved);
        Assert.Equal(user.UserId, saved.UserId);
        Assert.Equal(topic.TopicId, saved.TopicId);
    }

    [Fact]
    public async Task AnonymousBallot_ShouldNotContainUserId()
    {
        // Arrange
        await using var ctx = DbContextFactory.Create();
        var topic = new VoteTopic { Title = "停車場改建投票", EndTime = DateTime.UtcNow.AddDays(5) };
        ctx.VoteTopics.Add(topic);
        await ctx.SaveChangesAsync();

        // Act — AnonymousBallot 刻意不含 UserId
        var ballot = new AnonymousBallot
        {
            TopicId = topic.TopicId,
            OptionSelected = "贊成",
            HashToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
        };
        ctx.AnonymousBallots.Add(ballot);
        await ctx.SaveChangesAsync();

        // Assert
        var saved = await ctx.AnonymousBallots.FindAsync(ballot.BallotId);
        Assert.NotNull(saved);
        Assert.Equal("贊成", saved.OptionSelected);

        // 驗證 AnonymousBallot 沒有 UserId 屬性（編譯層防護）
        var properties = typeof(AnonymousBallot).GetProperties();
        Assert.DoesNotContain(properties, p => p.Name == "UserId");
    }

    [Fact]
    public async Task VoteStatuses_CountShouldReflectVoters()
    {
        // Arrange
        await using var ctx = DbContextFactory.Create();
        var topic = new VoteTopic { Title = "游泳池開放時間", EndTime = DateTime.UtcNow.AddDays(2) };
        ctx.VoteTopics.Add(topic);

        var users = new[]
        {
            new User { UnitNumber = "C301", UserName = "居民1", PasswordHash = "h1" },
            new User { UnitNumber = "C302", UserName = "居民2", PasswordHash = "h2" },
            new User { UnitNumber = "C303", UserName = "居民3", PasswordHash = "h3" }
        };
        ctx.Users.AddRange(users);
        await ctx.SaveChangesAsync();

        // Act — 3 位住戶分別投票
        foreach (var u in users)
        {
            ctx.VoteStatuses.Add(new VoteStatus
            {
                TopicId = topic.TopicId,
                UserId = u.UserId,
                VotedAt = DateTime.UtcNow
            });
        }
        await ctx.SaveChangesAsync();

        // Assert
        var count = ctx.VoteStatuses.Count(vs => vs.TopicId == topic.TopicId);
        Assert.Equal(3, count);
    }
}
