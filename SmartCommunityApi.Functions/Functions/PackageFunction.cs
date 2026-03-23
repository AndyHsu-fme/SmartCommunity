using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using SmartCommunityApi.Services;

namespace SmartCommunityApi.Functions.Functions;

[Authorize]
public class PackageFunction(IPackageService packageService)
{
    [Function("GetMyPackages")]
    public async Task<IActionResult> GetMyPackages(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "packages")] HttpRequest req)
    {
        var userId = GetCurrentUserId(req.HttpContext);
        var packages = await packageService.GetUserPackagesAsync(userId);
        return new OkObjectResult(packages);
    }

    [Function("PickupPackage")]
    public async Task<IActionResult> Pickup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "packages/{packageId}/pickup")] HttpRequest req,
        string packageId)
    {
        if (!int.TryParse(packageId, out var id))
            return new BadRequestObjectResult(new { message = "packageId 格式無效" });

        var success = await packageService.MarkPickedUpAsync(id);
        if (!success) return new BadRequestObjectResult(new { message = "包裹不存在或已領取" });
        return new OkObjectResult(new { message = "已確認領取" });
    }

    private static int GetCurrentUserId(HttpContext ctx) =>
        int.TryParse(ctx.User.FindFirstValue("sub") ??
                     ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}
