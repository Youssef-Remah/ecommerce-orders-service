namespace BusinessLogicLayer.DTOs
{
    public record OrderResponse(Guid OrderId, Guid UserId, decimal TotalBill, DateTime OrderDate, List<OrderItemResponse> OrderItems, string? UserName, string? Email)
    {
        public OrderResponse() : this(default, default, default, default, default, default, default)
        {
        }
    }
}
