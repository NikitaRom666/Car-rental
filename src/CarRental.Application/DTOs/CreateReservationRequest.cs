namespace CarRental.Application.DTOs;

/// <summary>
/// Input DTO for creating a new reservation.
/// </summary>
public class CreateReservationRequest
{
    public Guid CarId { get; init; }
    public Guid CustomerId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
