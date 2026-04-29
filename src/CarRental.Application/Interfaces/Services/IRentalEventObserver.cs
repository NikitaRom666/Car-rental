namespace CarRental.Application.Interfaces.Services;

/// <summary>
/// Observer pattern interface for logging rental events.
/// Allows decoupled event notification when rental state changes.
/// </summary>
public interface IRentalEventObserver
{
    Task OnRentalCreatedAsync(Guid rentalId, Guid customerId, Guid carId);
    Task OnRentalStartedAsync(Guid rentalId);
    Task OnRentalCompletedAsync(Guid rentalId);
    Task OnRentalCancelledAsync(Guid rentalId);
}

/// <summary>
/// Simple console logger implementing Observer pattern.
/// </summary>
public class ConsoleRentalEventObserver : IRentalEventObserver
{
    public Task OnRentalCreatedAsync(Guid rentalId, Guid customerId, Guid carId)
    {
        Console.WriteLine($"[EVENT] Rental created: ID={rentalId}, Customer={customerId}, Car={carId}");
        return Task.CompletedTask;
    }

    public Task OnRentalStartedAsync(Guid rentalId)
    {
        Console.WriteLine($"[EVENT] Rental started: ID={rentalId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        return Task.CompletedTask;
    }

    public Task OnRentalCompletedAsync(Guid rentalId)
    {
        Console.WriteLine($"[EVENT] Rental completed: ID={rentalId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        return Task.CompletedTask;
    }

    public Task OnRentalCancelledAsync(Guid rentalId)
    {
        Console.WriteLine($"[EVENT] Rental cancelled: ID={rentalId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        return Task.CompletedTask;
    }
}
