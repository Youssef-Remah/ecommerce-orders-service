namespace BusinessLogicLayer.DTOs
{
    public record OrderItemResponse(Guid ProductId, decimal UnitPrice, int Quantity, decimal TotalPrice)
    {
        public OrderItemResponse() : this(default, default, default, default)
        {
        }
    }
}