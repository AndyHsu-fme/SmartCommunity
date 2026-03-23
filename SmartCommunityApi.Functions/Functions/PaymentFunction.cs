using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using SmartCommunityApi.DTOs;
using SmartCommunityApi.Services;

namespace SmartCommunityApi.Functions.Functions;

[Authorize]
public class PaymentFunction(IPaymentService paymentService)
{
    [Function("CreatePaymentOrder")]
    public async Task<IActionResult> CreateOrder(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "payments/create-order")] HttpRequest req)
    {
        var request = await req.ReadFromJsonAsync<CreatePaymentOrderRequest>();
        if (request is null) return new BadRequestObjectResult(new { message = "請求格式錯誤" });

        var userId = GetCurrentUserId(req.HttpContext);
        var order = await paymentService.CreateOrderAsync(userId, request);
        return new OkObjectResult(order);
    }

    private static int GetCurrentUserId(HttpContext ctx) =>
        int.TryParse(ctx.User.FindFirstValue("sub") ??
                     ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}
