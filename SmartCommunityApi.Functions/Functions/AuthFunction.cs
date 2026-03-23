using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using SmartCommunityApi.DTOs;
using SmartCommunityApi.Services;

namespace SmartCommunityApi.Functions.Functions;

public class AuthFunction(IAuthService authService)
{
    [Function("Login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")] HttpRequest req)
    {
        var request = await req.ReadFromJsonAsync<LoginRequest>();
        if (request is null) return new BadRequestObjectResult(new { message = "請求格式錯誤" });

        var result = await authService.LoginAsync(request);
        if (result is null) return new UnauthorizedObjectResult(new { message = "門牌號碼或密碼錯誤" });
        return new OkObjectResult(result);
    }
}
