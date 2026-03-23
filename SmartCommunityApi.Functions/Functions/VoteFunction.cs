using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using SmartCommunityApi.DTOs;
using SmartCommunityApi.Services;

namespace SmartCommunityApi.Functions.Functions;

[Authorize]
public class VoteFunction(IVoteService voteService)
{
    [Function("GetVoteTopics")]
    public async Task<IActionResult> GetTopics(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "votes/topics")] HttpRequest req)
    {
        var userId = GetCurrentUserId(req.HttpContext);
        var topics = await voteService.GetActiveTopicsAsync(userId);
        return new OkObjectResult(topics);
    }

    [Function("CastVote")]
    public async Task<IActionResult> CastVote(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "votes/cast")] HttpRequest req)
    {
        var request = await req.ReadFromJsonAsync<CastVoteRequest>();
        if (request is null) return new BadRequestObjectResult(new { message = "請求格式錯誤" });

        var userId = GetCurrentUserId(req.HttpContext);
        var result = await voteService.CastVoteAsync(userId, request.TopicId, request.Option);
        return result switch
        {
            CastVoteResult.Success       => new OkObjectResult(new { message = "投票成功" }),
            CastVoteResult.AlreadyVoted  => new ConflictObjectResult(new { message = "您已投過此議題" }),
            CastVoteResult.TopicExpired  => new BadRequestObjectResult(new { message = "投票已截止" }),
            CastVoteResult.TopicNotFound => new NotFoundObjectResult(new { message = "投票議題不存在" }),
            _ => new StatusCodeResult(500)
        };
    }

    private static int GetCurrentUserId(HttpContext ctx) =>
        int.TryParse(ctx.User.FindFirstValue("sub") ??
                     ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}
