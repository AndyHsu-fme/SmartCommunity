using SmartCommunityApi.DTOs;

namespace SmartCommunityApi.Services;

public interface IPaymentService
{
    Task<string> CreatePaymentOrderAsync(decimal amount, string description);
    Task<PaymentOrderDto> CreateOrderAsync(int userId, CreatePaymentOrderRequest request);
}
