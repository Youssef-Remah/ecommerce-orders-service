namespace BusinessLogicLayer.DTOs
{
    public record OrderItemAddRequst(Guid ProductId, decimal UnitPrice, int Quantity)
    {
        public OrderItemAddRequst() : this(default, default, default)
        {
        }
    }
}