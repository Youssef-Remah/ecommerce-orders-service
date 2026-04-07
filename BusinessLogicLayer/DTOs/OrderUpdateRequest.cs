namespace BusinessLogicLayer.DTOs
{
    public record OrderUpdateRequest(Guid OrderId, Guid UserId, DateTime OrderDate, List<OrderItemAddRequst> OrderItems)
    {
        public OrderUpdateRequest() : this(default, default, default, default)
        {         
        }
    }
}
