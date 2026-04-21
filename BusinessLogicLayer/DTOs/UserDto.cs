namespace BusinessLogicLayer.DTOs
{
    public record UserDto(Guid UserID, string? Email, string? Name, string Gender);
}
