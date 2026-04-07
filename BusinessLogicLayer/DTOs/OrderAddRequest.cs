namespace BusinessLogicLayer.DTOs
{
    public record OrderAddRequest(Guid UserId, DateTime OrderDate, List<OrderItemAddRequst> OrderItems)
    {
        public OrderAddRequest() : this(default, default, default)
        {
            
        }
    }
}
