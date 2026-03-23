using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using SmartCommunityApi.DTOs;
using SmartCommunityApi.Services;

namespace SmartCommunityApi.Functions.Functions;

[Authorize]
public class ReservationFunction(IReservationService reservationService)
{
    [Function("GetFacilities")]
    public async Task<IActionResult> GetFacilities(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "reservations/facilities")] HttpRequest req)
    {
        var facilities = await reservationService.GetFacilitiesAsync();
        return new OkObjectResult(facilities);
    }

    [Function("CheckAvailability")]
    public async Task<IActionResult> CheckAvailability(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "reservations/availability")] HttpRequest req)
    {
        if (!int.TryParse(req.Query["facilityId"], out var facilityId))
            return new BadRequestObjectResult(new { message = "facilityId 無效" });

        if (!DateTime.TryParse(req.Query["start"], out var start) ||
            !DateTime.TryParse(req.Query["end"], out var end))
            return new BadRequestObjectResult(new { message = "時間格式無效" });

        bool available = await reservationService.CheckAvailabilityAsync(facilityId, start, end);
        return new OkObjectResult(new { available });
    }

    [Function("GetMyReservations")]
    public async Task<IActionResult> GetMyReservations(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "reservations")] HttpRequest req)
    {
        var userId = GetCurrentUserId(req.HttpContext);
        var list = await reservationService.GetUserReservationsAsync(userId);
        return new OkObjectResult(list);
    }

    [Function("CreateReservation")]
    public async Task<IActionResult> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reservations")] HttpRequest req)
    {
        var request = await req.ReadFromJsonAsync<CreateReservationRequest>();
        if (request is null) return new BadRequestObjectResult(new { message = "請求格式錯誤" });

        var userId = GetCurrentUserId(req.HttpContext);
        var (result, dto) = await reservationService.CreateReservationAsync(userId, request);
        return result switch
        {
            CreateReservationResult.Success          => new OkObjectResult(dto),
            CreateReservationResult.Conflict         => new ConflictObjectResult(new { message = "該時段已被預約，請選擇其他時間" }),
            CreateReservationResult.FacilityNotFound => new NotFoundObjectResult(new { message = "設施不存在" }),
            _ => new StatusCodeResult(500)
        };
    }

    [Function("CancelReservation")]
    public async Task<IActionResult> Cancel(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reservations/{reservationId}/cancel")] HttpRequest req,
        string reservationId)
    {
        if (!int.TryParse(reservationId, out var id))
            return new BadRequestObjectResult(new { message = "reservationId 格式無效" });

        var userId = GetCurrentUserId(req.HttpContext);
        var success = await reservationService.CancelReservationAsync(id, userId);
        if (!success) return new BadRequestObjectResult(new { message = "預約不存在或已取消" });
        return new OkObjectResult(new { message = "預約已取消" });
    }

    private static int GetCurrentUserId(HttpContext ctx) =>
        int.TryParse(ctx.User.FindFirstValue("sub") ??
                     ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}
