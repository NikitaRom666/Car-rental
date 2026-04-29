namespace CarRental.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using CarRental.Application.Interfaces.Repositories;
using CarRental.Application.Interfaces.Services;
using CarRental.Infrastructure.Persistence;

/// <summary>
/// Extension methods for registering infrastructure dependencies.
/// Demonstrates Dependency Injection Container setup (SOLID - Dependency Inversion).
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        string dataDirectory = "data")
    {
        // Register repositories with JSON persistence
        services.AddSingleton<ICarRepository>(new JsonCarRepository(dataDirectory));
        services.AddSingleton<ICustomerRepository>(new JsonCustomerRepository(dataDirectory));
        services.AddSingleton<IRentalRepository>(new JsonRentalRepository(dataDirectory));
        services.AddSingleton<IReservationRepository>(new JsonReservationRepository(dataDirectory));
        services.AddSingleton<IPaymentRepository>(new JsonPaymentRepository(dataDirectory));
        services.AddSingleton<ILocationRepository>(new JsonLocationRepository(dataDirectory));

        // Register observers for event logging
        services.AddSingleton<IRentalEventObserver, ConsoleRentalEventObserver>();

        return services;
    }
}
