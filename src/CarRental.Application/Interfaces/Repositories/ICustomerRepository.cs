namespace CarRental.Application.Interfaces.Repositories;
using CarRental.Domain.Entities;

/// <summary>
/// Repository interface for Customer aggregate (Repository pattern).
/// </summary>
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id);
    Task<Customer?> GetByEmailAsync(string email);
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<bool> ExistsAsync(Guid id);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Guid id);
}
