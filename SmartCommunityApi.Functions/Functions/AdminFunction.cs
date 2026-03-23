using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using SmartCommunityApi.DTOs;
using SmartCommunityApi.Services;

namespace SmartCommunityApi.Functions.Functions;

[Authorize]
public class AdminFunction(
    IVoteService voteService,
    IUserService userService,
    IPackageService packageService)
{
    private static bool IsAdmin(HttpContext ctx) =>
        string.Equals(ctx.User.FindFirstValue("isAdmin"), "true", StringComparison.OrdinalIgnoreCase);

    [Function("AdminGetVoteTopics")]
    public async Task<IActionResult> GetVoteTopics(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "admin/vote-topics")] HttpRequest req)
    {
        if (!IsAdmin(req.HttpContext)) return new ObjectResult(new { message = "權限不足" }) { StatusCode = 403 };
        return new OkObjectResult(await voteService.GetAllTopicsWithResultsAsync());
    }

    [Function("AdminCreateVoteTopic")]
    public async Task<IActionResult> CreateVoteTopic(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "admin/vote-topics")] HttpRequest req)
    {
        if (!IsAdmin(req.HttpContext)) return new ObjectResult(new { message = "權限不足" }) { StatusCode = 403 };
        var request = await req.ReadFromJsonAsync<CreateVoteTopicRequest>();
        if (request is null) return new BadRequestObjectResult(new { message = "請求格式錯誤" });
        var result = await voteService.CreateTopicAsync(request);
        return new OkObjectResult(result);
    }

    [Function("AdminGetUsers")]
    public async Task<IActionResult> GetUsers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "admin/users")] HttpRequest req)
    {
        if (!IsAdmin(req.HttpContext)) return new ObjectResult(new { message = "權限不足" }) { StatusCode = 403 };
        return new OkObjectResult(await userService.GetAllUsersAsync());
    }

    [Function("AdminCreateUser")]
    public async Task<IActionResult> CreateUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "admin/users")] HttpRequest req)
    {
        if (!IsAdmin(req.HttpContext)) return new ObjectResult(new { message = "權限不足" }) { StatusCode = 403 };
        var request = await req.ReadFromJsonAsync<CreateUserRequest>();
        if (request is null) return new BadRequestObjectResult(new { message = "請求格式錯誤" });
        var (success, error, dto) = await userService.CreateUserAsync(request);
        if (!success) return new ConflictObjectResult(new { message = error });
        return new OkObjectResult(dto);
    }

    [Function("AdminCreatePackage")]
    public async Task<IActionResult> CreatePackage(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "admin/packages")] HttpRequest req)
    {
        if (!IsAdmin(req.HttpContext)) return new ObjectResult(new { message = "權限不足" }) { StatusCode = 403 };
        var request = await req.ReadFromJsonAsync<CreatePackageRequest>();
        if (request is null) return new BadRequestObjectResult(new { message = "請求格式錯誤" });
        var dto = await packageService.CreatePackageAsync(request);
        return new OkObjectResult(dto);
    }
}
