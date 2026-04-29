namespace CarRental.Application.Interfaces.Repositories;
using CarRental.Domain.Entities;

/// <summary>
/// Repository interface for Payment aggregate (Repository pattern).
/// </summary>
public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Payment>> GetByRentalIdAsync(Guid rentalId);
    Task<IEnumerable<Payment>> GetAllAsync();
    Task AddAsync(Payment payment);
    Task UpdateAsync(Payment payment);
    Task DeleteAsync(Guid id);
}
