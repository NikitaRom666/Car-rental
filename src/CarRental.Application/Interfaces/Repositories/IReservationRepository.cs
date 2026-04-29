namespace CarRental.Application.Interfaces.Repositories;
using CarRental.Domain.Entities;
using CarRental.Domain.ValueObjects;

/// <summary>
/// Repository interface for Reservation aggregate (Repository pattern).
/// </summary>
public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id);
    Task<IEnumerable<Reservation>> GetAllAsync();
    Task<IEnumerable<Reservation>> GetByCarIdAsync(Guid carId);
    Task<IEnumerable<Reservation>> GetActiveReservationsForCarAsync(Guid carId);
    Task<bool> HasConflictingReservationsAsync(Guid carId, DateRange period);
    Task AddAsync(Reservation reservation);
    Task UpdateAsync(Reservation reservation);
    Task DeleteAsync(Guid id);
}
