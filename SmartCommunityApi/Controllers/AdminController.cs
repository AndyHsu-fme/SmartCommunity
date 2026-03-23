using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCommunityApi.DTOs;
using SmartCommunityApi.Services;

namespace SmartCommunityApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController(
    IVoteService voteService,
    IUserService userService,
    IPackageService packageService) : ControllerBase
{
    private bool IsAdmin =>
        string.Equals(User.FindFirstValue("isAdmin"), "true", StringComparison.OrdinalIgnoreCase);

    // ── 投票管理 ──────────────────────────────────────────────────────────────

    [HttpGet("vote-topics")]
    public async Task<IActionResult> GetVoteTopics()
    {
        if (!IsAdmin) return Forbid();
        return Ok(await voteService.GetAllTopicsWithResultsAsync());
    }

    [HttpPost("vote-topics")]
    public async Task<IActionResult> CreateVoteTopic([FromBody] CreateVoteTopicRequest request)
    {
        if (!IsAdmin) return Forbid();
        var result = await voteService.CreateTopicAsync(request);
        return CreatedAtAction(nameof(GetVoteTopics), result);
    }

    // ── 住戶管理 ──────────────────────────────────────────────────────────────

    /// <summary>取得所有住戶列表</summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        if (!IsAdmin) return Forbid();
        return Ok(await userService.GetAllUsersAsync());
    }

    /// <summary>新增住戶（密碼由後端 BCrypt 雜湊）</summary>
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!IsAdmin) return Forbid();
        var (success, error, dto) = await userService.CreateUserAsync(request);
        if (!success)
            return Conflict(new { message = error });
        return CreatedAtAction(nameof(GetUsers), dto);
    }

    // ── 包裹管理 ──────────────────────────────────────────────────────────────

    /// <summary>管理員登記新到包裹</summary>
    [HttpPost("packages")]
    public async Task<IActionResult> CreatePackage([FromBody] CreatePackageRequest request)
    {
        if (!IsAdmin) return Forbid();
        var dto = await packageService.CreatePackageAsync(request);
        return CreatedAtAction(nameof(CreatePackage), dto);
    }
}
