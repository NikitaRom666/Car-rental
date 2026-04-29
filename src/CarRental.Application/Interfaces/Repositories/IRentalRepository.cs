namespace CarRental.Application.Interfaces.Repositories;
using CarRental.Domain.Entities;
using CarRental.Domain.ValueObjects;

/// <summary>
/// Repository interface for Rental aggregate (Repository pattern).
/// </summary>
public interface IRentalRepository
{
    Task<Rental?> GetByIdAsync(Guid id);
    Task<IEnumerable<Rental>> GetAllAsync();
    Task<IEnumerable<Rental>> GetByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<Rental>> GetActiveRentalsForCarAsync(Guid carId);
    Task<bool> HasConflictingRentalsAsync(Guid carId, DateRange period);
    Task AddAsync(Rental rental);
    Task UpdateAsync(Rental rental);
    Task DeleteAsync(Guid id);
}
