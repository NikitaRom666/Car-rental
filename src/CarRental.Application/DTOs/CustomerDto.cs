namespace CarRental.Application.DTOs;

/// <summary>
/// DTO for displaying customer information.
/// </summary>
public class CustomerDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
