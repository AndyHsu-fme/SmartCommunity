using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCommunityApi.DTOs;
using SmartCommunityApi.Services;

namespace SmartCommunityApi.Controllers;

[ApiController]
[Route("api/votes")]
[Authorize]
public class VoteController(IVoteService voteService) : ControllerBase
{
    private int CurrentUserId =>
        int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    [HttpGet("topics")]
    public async Task<IActionResult> GetTopics()
    {
        var topics = await voteService.GetActiveTopicsAsync(CurrentUserId);
        return Ok(topics);
    }

    [HttpPost("cast")]
    public async Task<IActionResult> CastVote([FromBody] CastVoteRequest request)
    {
        var result = await voteService.CastVoteAsync(CurrentUserId, request.TopicId, request.Option);
        return result switch
        {
            CastVoteResult.Success      => Ok(new { message = "投票成功" }),
            CastVoteResult.AlreadyVoted => Conflict(new { message = "您已投過此議題" }),
            CastVoteResult.TopicExpired => BadRequest(new { message = "投票已截止" }),
            CastVoteResult.TopicNotFound => NotFound(new { message = "投票議題不存在" }),
            _ => StatusCode(500)
        };
    }
}
