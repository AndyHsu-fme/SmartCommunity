using SmartCommunityApi.DTOs;

namespace SmartCommunityApi.Services;

/// <summary>
/// 管理費支付服務（目前為 Stub 實作，待串接第三方金流）
/// </summary>
public class PaymentService : IPaymentService
{
    public Task<string> CreatePaymentOrderAsync(decimal amount, string description)
    {
        var orderId = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        return Task.FromResult(orderId);
    }

    public Task<PaymentOrderDto> CreateOrderAsync(int userId, CreatePaymentOrderRequest request)
    {
        var orderId = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        var order = new PaymentOrderDto
        {
            OrderId     = orderId,
            Amount      = request.Amount,
            Description = request.Description,
            CreatedAt   = DateTime.UtcNow,
        };
        return Task.FromResult(order);
    }
}
