using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCommunityApi.Services;

namespace SmartCommunityApi.Controllers;

[ApiController]
[Route("api/packages")]
[Authorize]
public class PackageController(IPackageService packageService) : ControllerBase
{
    private int CurrentUserId =>
        int.TryParse(User.FindFirstValue("sub")
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    /// <summary>取得目前住戶的所有包裹</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyPackages()
    {
        var packages = await packageService.GetUserPackagesAsync(CurrentUserId);
        return Ok(packages);
    }

    /// <summary>確認領取包裹</summary>
    [HttpPost("{packageId:int}/pickup")]
    public async Task<IActionResult> Pickup(int packageId)
    {
        var success = await packageService.MarkPickedUpAsync(packageId);
        if (!success)
            return BadRequest(new { message = "包裹不存在或已領取" });
        return Ok(new { message = "已確認領取" });
    }
}
