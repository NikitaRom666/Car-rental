namespace CarRental.Application.Interfaces.Repositories;
using CarRental.Domain.Entities;
using CarRental.Domain.ValueObjects;

/// <summary>
/// Repository interface for Car aggregate (Repository pattern).
/// Defines contract for data access operations on cars.
/// </summary>
public interface ICarRepository
{
    Task<Car?> GetByIdAsync(Guid id);
    Task<IEnumerable<Car>> GetAllAsync();
    Task<IEnumerable<Car>> GetAvailableCarsAsync();
    Task<IEnumerable<Car>> GetCarsByCategoryAsync(int category);
    Task<bool> ExistsAsync(Guid id);
    Task AddAsync(Car car);
    Task UpdateAsync(Car car);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<Car>> GetCarsAvailableForPeriodAsync(DateRange period);
}
