using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCommunityApi.DTOs;
using SmartCommunityApi.Services;

namespace SmartCommunityApi.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentController(IPaymentService paymentService) : ControllerBase
{
    private int CurrentUserId =>
        int.TryParse(User.FindFirstValue("sub")
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    /// <summary>建立管理費支付訂單</summary>
    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreatePaymentOrderRequest request)
    {
        var order = await paymentService.CreateOrderAsync(CurrentUserId, request);
        return Ok(order);
    }
}
