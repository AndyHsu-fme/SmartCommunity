namespace SmartCommunityApi.DTOs;

public record CreatePaymentOrderRequest(decimal Amount, string Description);

public class PaymentOrderDto
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
