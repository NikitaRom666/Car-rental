namespace CarRental.Application.Interfaces.Repositories;
using CarRental.Domain.Entities;

/// <summary>
/// Repository interface for Location aggregate.
/// </summary>
public interface ILocationRepository
{
    Task<Location?> GetByIdAsync(Guid id);
    Task<IEnumerable<Location>> GetAllAsync();
    Task<bool> ExistsAsync(Guid id);
    Task AddAsync(Location location);
    Task UpdateAsync(Location location);
    Task DeleteAsync(Guid id);
}
