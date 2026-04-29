namespace CarRental.Application.DTOs;
using CarRental.Domain.Enums;

/// <summary>
/// DTO for displaying rental information.
/// </summary>
public class RentalDto
{
    public Guid Id { get; init; }
    public Guid CarId { get; init; }
    public Guid CustomerId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int DaysCount { get; init; }
    public decimal TotalPrice { get; init; }
    public RentalStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string StatusName => Enum.GetName(typeof(RentalStatus), Status) ?? "Unknown";
}
