namespace CarRental.Application.DTOs;
using CarRental.Domain.Enums;

/// <summary>
/// DTO for displaying car information to the UI layer.
/// </summary>
public class CarDto
{
    public Guid Id { get; init; }
    public string Model { get; init; } = string.Empty;
    public CarCategory Category { get; init; }
    public decimal PricePerDay { get; init; }
    public bool IsAvailable { get; init; }
    public string LicensePlate { get; init; } = string.Empty;
    public int ManufacturingYear { get; init; }
    public string CategoryName => Enum.GetName(typeof(CarCategory), Category) ?? "Unknown";
    public string AvailabilityStatus => IsAvailable ? "Available" : "Not Available";
}
