using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCommunityApi.DTOs;
using SmartCommunityApi.Services;

namespace SmartCommunityApi.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationController(IReservationService reservationService) : ControllerBase
{
    private int CurrentUserId =>
        int.TryParse(User.FindFirstValue("sub")
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    /// <summary>取得所有可用設施</summary>
    [HttpGet("facilities")]
    public async Task<IActionResult> GetFacilities()
    {
        var facilities = await reservationService.GetFacilitiesAsync();
        return Ok(facilities);
    }

    /// <summary>查詢時段是否可預約</summary>
    [HttpGet("availability")]
    public async Task<IActionResult> CheckAvailability(
        [FromQuery] int facilityId,
        [FromQuery] DateTime start,
        [FromQuery] DateTime end)
    {
        bool available = await reservationService.CheckAvailabilityAsync(facilityId, start, end);
        return Ok(new { available });
    }

    /// <summary>取得目前住戶的預約紀錄</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyReservations()
    {
        var list = await reservationService.GetUserReservationsAsync(CurrentUserId);
        return Ok(list);
    }

    /// <summary>新增預約（含時間衝突偵測）</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationRequest request)
    {
        var (result, dto) = await reservationService.CreateReservationAsync(CurrentUserId, request);
        return result switch
        {
            CreateReservationResult.Success         => CreatedAtAction(nameof(GetMyReservations), dto),
            CreateReservationResult.Conflict        => Conflict(new { message = "該時段已被預約，請選擇其他時間" }),
            CreateReservationResult.FacilityNotFound => NotFound(new { message = "設施不存在" }),
            _ => StatusCode(500)
        };
    }

    /// <summary>取消預約</summary>
    [HttpPost("{reservationId:int}/cancel")]
    public async Task<IActionResult> Cancel(int reservationId)
    {
        var success = await reservationService.CancelReservationAsync(reservationId, CurrentUserId);
        if (!success)
            return BadRequest(new { message = "預約不存在或已取消" });
        return Ok(new { message = "預約已取消" });
    }
}
