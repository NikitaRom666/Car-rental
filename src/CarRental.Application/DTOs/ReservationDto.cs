namespace CarRental.Application.DTOs;
using CarRental.Domain.Enums;

/// <summary>
/// DTO for displaying reservation information.
/// </summary>
public class ReservationDto
{
    public Guid Id { get; init; }
    public Guid CarId { get; init; }
    public Guid CustomerId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int DaysCount { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
