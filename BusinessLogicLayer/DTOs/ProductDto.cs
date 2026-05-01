namespace BusinessLogicLayer.DTOs
{
    public record ProductDto(Guid ProductID, string? ProductName, string? Category, double UnitPrice, int QuantityInStock);
}
