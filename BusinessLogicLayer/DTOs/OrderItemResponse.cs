namespace BusinessLogicLayer.DTOs
{
    public record OrderItemResponse(Guid ProductId, decimal UnitPrice, int Quantity, decimal TotalPrice, string? ProductName, string? Category)
    {
        public OrderItemResponse() : this(default, default, default, default, default, default)
        {
        }
    }
}